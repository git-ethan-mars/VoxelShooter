using System;
using System.Collections.Generic;
using Networking;
using Networking.Messages;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;

namespace GamePlay
{
	// Keeps every map column as vertical runs of solid voxels plus a graph of touching runs.
	// After voxels are removed, runs that can no longer reach the ground (y = 0) are removed as floating.
	public class ColumnDestructionAlgorithm : MapFeature
	{
		private const int MaxVoxelsPerMessage = 1000;

		private readonly Dictionary<Run, HashSet<Run>> _neighboursByRun = new Dictionary<Run, HashSet<Run>>();

		private List<Run>[] _columns;
		private MapProvider _mapProvider;
		private VSNetworkManager _networkManager;
		private int _width;
		private int _height;
		private int _depth;

		[Inject]
		private void Construct(MapProvider mapProvider, VSNetworkManager networkManager)
		{
			_mapProvider = mapProvider;
			_networkManager = networkManager;
		}

		private void Start()
		{
			Map map = _mapProvider.Map.CurrentValue;
			_width = map.Width;
			_height = map.Height;
			_depth = map.Depth;
			_columns = new List<Run>[_width * _depth];

			PreProcessColumns(map);
			PreProcessGraph();

			map.VoxelsAdded
				.Subscribe(Add)
				.AddTo(this);
			map.VoxelsRemoved
				.Subscribe(Remove)
				.AddTo(this);
		}

		private void Add(IReadOnlyList<Voxel> voxels)
		{
			foreach (Voxel voxel in voxels)
			{
				Vector3Ushort position = voxel.Position;

				if (TryFindRunInColumn(position.x, position.y, position.z, out _))
				{
					continue;
				}

				bool hasRunBelow = TryFindRunInColumn(position.x, position.y - 1, position.z, out Run runBelow);
				bool hasRunAbove = TryFindRunInColumn(position.x, position.y + 1, position.z, out Run runAbove);

				if (hasRunBelow && hasRunAbove)
				{
					RemoveRun(runBelow);
					RemoveRun(runAbove);
					AddRun(new Run(runBelow.X, runBelow.Z, runBelow.Begin, (ushort)(runBelow.Length + 1 + runAbove.Length),
						runBelow.IsCreatedByPlayer));
				}
				else if (hasRunBelow)
				{
					RemoveRun(runBelow);
					AddRun(new Run(runBelow.X, runBelow.Z, runBelow.Begin, (ushort)(runBelow.Length + 1), runBelow.IsCreatedByPlayer));
				}
				else if (hasRunAbove)
				{
					RemoveRun(runAbove);
					AddRun(new Run(runAbove.X, runAbove.Z, position.y, (ushort)(runAbove.Length + 1), runAbove.IsCreatedByPlayer));
				}
				else
				{
					AddRun(new Run(position.x, position.z, position.y, 1, true));
				}
			}
		}

		private void Remove(IReadOnlyList<Vector3Ushort> removedPositions)
		{
			using PooledObject<HashSet<Run>> candidatesSet = HashSetPool<Run>.Get(out HashSet<Run> candidates);
			using PooledObject<HashSet<Run>> groundedSet = HashSetPool<Run>.Get(out HashSet<Run> grounded);
			using PooledObject<HashSet<Run>> floatingSet = HashSetPool<Run>.Get(out HashSet<Run> floating);
			using PooledObject<List<Voxel>> fallingVoxelsList = ListPool<Voxel>.Get(out List<Voxel> fallingVoxels);

			foreach (Vector3Ushort position in removedPositions)
			{
				if (!TryFindRunInColumn(position.x, position.y, position.z, out Run run))
				{
					continue;
				}

				RemoveRun(run);
				AddRun(new Run(run.X, run.Z, run.Begin, (ushort)(position.y - run.Begin), run.IsCreatedByPlayer));
				AddRun(new Run(run.X, run.Z, (ushort)(position.y + 1), (ushort)(run.End - position.y), run.IsCreatedByPlayer));
			}

			// Only runs around the removed voxels can have lost their support.
			foreach (Vector3Ushort position in removedPositions)
			{
				CollectRunsAround(position, candidates);
			}

			using PooledObject<List<Run>> componentList = ListPool<Run>.Get(out List<Run> component);

			foreach (Run candidate in candidates)
			{
				if (grounded.Contains(candidate) || floating.Contains(candidate) || !_neighboursByRun.ContainsKey(candidate))
				{
					continue;
				}

				if (TryFindFloatingComponent(candidate, grounded, floating, component))
				{
					DetachComponent(component, fallingVoxels);
				}
			}

			if (fallingVoxels.Count > 0)
			{
				_mapProvider.Map.CurrentValue.SetVoxelsByGlobalPositions(fallingVoxels);
			}
		}

		// Depth-first search that always continues with the lowest run first, so a supported
		// component usually reaches the ground in a few steps and the search stops right there.
		private bool TryFindFloatingComponent(Run start, HashSet<Run> grounded, HashSet<Run> floating, List<Run> component)
		{
			component.Clear();
			using PooledObject<HashSet<Run>> visitedSet = HashSetPool<Run>.Get(out HashSet<Run> visited);
			using PooledObject<List<Run>> pathList = ListPool<Run>.Get(out List<Run> path);
			using PooledObject<List<Run>> nextRunsList = ListPool<Run>.Get(out List<Run> nextRuns);
			path.Add(start);

			while (path.Count > 0)
			{
				Run current = path[^1];
				path.RemoveAt(path.Count - 1);

				if (!visited.Add(current))
				{
					continue;
				}

				if (current.Begin == 0 || grounded.Contains(current))
				{
					grounded.UnionWith(visited);
					return false;
				}

				nextRuns.Clear();

				foreach (Run neighbour in _neighboursByRun[current])
				{
					if (!visited.Contains(neighbour))
					{
						nextRuns.Add(neighbour);
					}
				}

				// Highest first, so the lowest run ends on top of the stack.
				nextRuns.Sort((first, second) => second.Begin.CompareTo(first.Begin));
				path.AddRange(nextRuns);
			}

			floating.UnionWith(visited);
			component.AddRange(visited);
			return true;
		}

		// Turns a floating component into air and sends its voxels to clients, which let it fall as one mesh.
		private void DetachComponent(List<Run> component, List<Voxel> fallingVoxels)
		{
			Map map = _mapProvider.Map.CurrentValue;
			var componentVoxels = new List<Voxel>();

			foreach (Run run in component)
			{
				for (int y = run.Begin; y <= run.End; y++)
				{
					var position = new Vector3Ushort(run.X, (ushort)y, run.Z);
					componentVoxels.Add(new Voxel(position, map.GetVoxelByGlobalPosition(position)));
					fallingVoxels.Add(new Voxel(position, VoxelData.Air));
				}

				RemoveRun(run);
			}

			for (int start = 0; start < componentVoxels.Count; start += MaxVoxelsPerMessage)
			{
				int count = Math.Min(MaxVoxelsPerMessage, componentVoxels.Count - start);
				_networkManager.SendResponseToAll(new FallingVoxelsResponse(componentVoxels.GetRange(start, count)));
			}
		}

		private void AddRun(Run run)
		{
			if (run.Length == 0)
			{
				return;
			}

			_columns[GetColumnIndex(run.X, run.Z)].Add(run);
			var neighbours = new HashSet<Run>();
			_neighboursByRun[run] = neighbours;

			for (int x = run.X - 1; x <= run.X + 1; x++)
			{
				for (int z = run.Z - 1; z <= run.Z + 1; z++)
				{
					if (!IsInsideColumns(x, z))
					{
						continue;
					}

					foreach (Run other in _columns[GetColumnIndex(x, z)])
					{
						if (other != run && AreConnected(run, other))
						{
							neighbours.Add(other);
							_neighboursByRun[other].Add(run);
						}
					}
				}
			}
		}

		private void RemoveRun(Run run)
		{
			_columns[GetColumnIndex(run.X, run.Z)].Remove(run);

			foreach (Run neighbour in _neighboursByRun[run])
			{
				_neighboursByRun[neighbour].Remove(run);
			}

			_neighboursByRun.Remove(run);
		}

		private void PreProcessColumns(Map map)
		{
			for (ushort x = 0; x < _width; x++)
			{
				for (ushort z = 0; z < _depth; z++)
				{
					var runs = new List<Run>();
					int begin = -1;

					for (int y = 0; y <= _height; y++)
					{
						bool isSolid = y < _height && map.GetVoxelByGlobalPosition(x, (ushort)y, z).IsSolid();

						if (isSolid && begin < 0)
						{
							begin = y;
						}
						else if (!isSolid && begin >= 0)
						{
							runs.Add(new Run(x, z, (ushort)begin, (ushort)(y - begin), false));
							begin = -1;
						}
					}

					_columns[GetColumnIndex(x, z)] = runs;
				}
			}
		}

		private void PreProcessGraph()
		{
			foreach (List<Run> column in _columns)
			{
				foreach (Run run in column)
				{
					_neighboursByRun[run] = new HashSet<Run>();
				}
			}

			// Runs of one column never touch, so only neighbouring columns are compared, each pair once.
			for (int x = 0; x < _width; x++)
			{
				for (int z = 0; z < _depth; z++)
				{
					foreach (Run run in _columns[GetColumnIndex(x, z)])
					{
						ConnectWithColumn(run, x + 1, z - 1);
						ConnectWithColumn(run, x + 1, z);
						ConnectWithColumn(run, x + 1, z + 1);
						ConnectWithColumn(run, x, z + 1);
					}
				}
			}
		}

		private void ConnectWithColumn(Run run, int x, int z)
		{
			if (!IsInsideColumns(x, z))
			{
				return;
			}

			foreach (Run other in _columns[GetColumnIndex(x, z)])
			{
				if (AreConnected(run, other))
				{
					_neighboursByRun[run].Add(other);
					_neighboursByRun[other].Add(run);
				}
			}
		}

		// Map voxels hold on to anything they touch, even diagonally; voxels placed by players only hold by faces.
		private static bool AreConnected(Run first, Run second)
		{
			int deltaX = Math.Abs(first.X - second.X);
			int deltaZ = Math.Abs(first.Z - second.Z);

			if (deltaX > 1 || deltaZ > 1)
			{
				return false;
			}

			if (deltaX == 0 && deltaZ == 0)
			{
				return first.End + 1 == second.Begin || second.End + 1 == first.Begin;
			}

			bool isFaceOnly = first.IsCreatedByPlayer || second.IsCreatedByPlayer;

			if (isFaceOnly && deltaX + deltaZ != 1)
			{
				return false;
			}

			int tolerance = isFaceOnly ? 0 : 1;
			return first.Begin <= second.End + tolerance && second.Begin <= first.End + tolerance;
		}

		private void CollectRunsAround(Vector3Ushort position, HashSet<Run> runs)
		{
			for (int x = position.x - 1; x <= position.x + 1; x++)
			{
				for (int z = position.z - 1; z <= position.z + 1; z++)
				{
					if (!IsInsideColumns(x, z))
					{
						continue;
					}

					foreach (Run run in _columns[GetColumnIndex(x, z)])
					{
						if (run.Begin <= position.y + 1 && position.y - 1 <= run.End)
						{
							runs.Add(run);
						}
					}
				}
			}
		}

		private bool TryFindRunInColumn(int x, int y, int z, out Run run)
		{
			if (IsInsideColumns(x, z) && y >= 0 && y < _height)
			{
				foreach (Run columnRun in _columns[GetColumnIndex(x, z)])
				{
					if (columnRun.Begin <= y && y <= columnRun.End)
					{
						run = columnRun;
						return true;
					}
				}
			}

			run = default;
			return false;
		}

		private bool IsInsideColumns(int x, int z)
		{
			return x >= 0 && x < _width && z >= 0 && z < _depth;
		}

		private int GetColumnIndex(int x, int z)
		{
			return x * _depth + z;
		}

		private readonly struct Run : IEquatable<Run>
		{
			public readonly bool IsCreatedByPlayer;
			public readonly ushort X;
			public readonly ushort Z;
			public readonly ushort Begin;
			public readonly ushort Length;

			public Run(ushort x, ushort z, ushort begin, ushort length, bool isCreatedByPlayer)
			{
				X = x;
				Z = z;
				Begin = begin;
				Length = length;
				IsCreatedByPlayer = isCreatedByPlayer;
			}

			public int End => Begin + Length - 1;

			public bool Equals(Run other)
			{
				return IsCreatedByPlayer == other.IsCreatedByPlayer && X == other.X && Z == other.Z && Begin == other.Begin &&
				       Length == other.Length;
			}

			public override bool Equals(object obj)
			{
				return obj is Run other && Equals(other);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(IsCreatedByPlayer, X, Z, Begin, Length);
			}

			public static bool operator ==(Run left, Run right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Run left, Run right)
			{
				return !left.Equals(right);
			}

			public override string ToString()
			{
				return $"RUN (X: {X}, Z: {Z}, BEGIN: {Begin}, LENGTH: {Length})";
			}
		}
	}
}
