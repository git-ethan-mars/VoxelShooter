using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class ColumnDestructionAlgorithm : MapFeature
	{
		private Dictionary<Run, HashSet<Run>> _neighboursByRun;
		private List<Run>[] _columns;
		private MapProvider _mapProvider;

		[Inject]
		private void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
		}

		private void Start()
		{
			_columns = new List<Run>[_mapProvider.Map.Width * _mapProvider.Map.Depth];
			_neighboursByRun = new Dictionary<Run, HashSet<Run>>();

			for (var i = 0; i < _columns.Length; i++)
			{
				_columns[i] = new List<Run>();
			}

			PreProcessColumns();
			PreProcessGraph();

			_mapProvider.Map.VoxelsAdded
				.Subscribe(Add)
				.AddTo(this);
			_mapProvider.Map.VoxelsRemoved
				.Subscribe(Remove)
				.AddTo(this);
		}

		private void Add(IReadOnlyList<Voxel> voxels)
		{
			for (var i = 0; i < voxels.Count; i++)
			{
				if (!TryMergeWithExistingRuns(voxels[i]))
				{
					var singleVoxelRun = new Run(voxels[i].Position.x, voxels[i].Position.z, voxels[i].Position.y, 1, true);
					AddRun(singleVoxelRun);
				}
			}
		}

		private void Remove(IReadOnlyList<Vector3Ushort> removingPositions)
		{
			using var neighbourPositions = ListPool<Vector3Ushort>.Get(out var neighboursList);
			using var runsSet = HashSetPool<Run>.Get(out var runs);
			using var deletingRunsList = ListPool<Run>.Get(out var deletingRuns);
			using var fallingVoxelsList = ListPool<Voxel>.Get(out var fallingVoxels);

			for (int i = 0; i < removingPositions.Count; i++)
			{
				if (!TryFindRunInColumn(removingPositions[i], out Run run))
				{
					continue;
				}
				
				if (removingPositions[i].y == run.Begin || removingPositions[i].y == run.Begin + run.Length - 1)
				{
					var newRun = new Run(run.X, run.Z, removingPositions[i].y == run.Begin ? (ushort)(run.Begin + 1) : run.Begin,
						(ushort)(run.Length - 1), run.IsCreatedByPlayer);
					RemoveRun(run);
					AddRun(newRun);
				}
				else
			 	{
					SplitRun(run, removingPositions[i].y, out Run firstRun, out Run secondRun);
					RemoveRun(run);
					AddRun(firstRun);
					AddRun(secondRun);
				}

				foreach (Vector3Ushort neighbour in GetConnectedNeighbours(removingPositions[i]))
				{
					neighboursList.Add(neighbour);
				}
			}

			foreach (Vector3Ushort neighbour in neighboursList)
			{
				if (TryFindRunInColumn(neighbour, out Run run))
				{
					runs.Add(run);
				}
			}

			var path = new Stack<Run>();
			while (runs.Count > 0)
			{
				using var visitedSet = HashSetPool<Run>.Get(out var visited);
				Run startRun = runs.First();
				path.Push(startRun);
				var isSeparatedComponent = true;

				while (path.Count > 0)
				{
					Run currentRun = path.Pop();
					visited.Add(currentRun);
					runs.Remove(currentRun);

					foreach (Run nextRun in _neighboursByRun[currentRun])
					{
						if (visited.Contains(nextRun))
						{
							continue;
						}

						if (nextRun.Begin == 0)
						{
							isSeparatedComponent = false;
							break;
						}

						path.Push(nextRun);
					}
				}

				if (isSeparatedComponent)
				{
					deletingRuns.AddRange(visited);
				}
			}

			foreach (Run deletingRun in deletingRuns)
			{
				foreach (Vector3Ushort fallingPosition in GetPositionsInRun(deletingRun))
				{
					fallingVoxels.Add(new Voxel(fallingPosition, VoxelData.Air));
				}
			
				RemoveRun(deletingRun);
			}

			if (fallingVoxels.Count > 0)
			{	
				Debug.Log(fallingVoxels.Count);
				_mapProvider.Map.SetVoxelsByGlobalPositions(fallingVoxels);
			}
		}

		private void AddRun(Run run)
		{
			_columns[run.X * _mapProvider.Map.Depth + run.Z].Add(run);
			_neighboursByRun[run] = new HashSet<Run>();

			using var pooledObject = ListPool<Vector3Ushort>.Get(out var neighbourPositions);

			for (ushort y = run.Begin; y < run.Begin + run.Length; y++)
			{
				var position = new Vector3Ushort(run.X, y, run.Z);

				foreach (Vector3Ushort neighbour in run.IsCreatedByPlayer ? GetConnectedNeighboursWithoutDiagonals(position)
					         : GetConnectedNeighbours(position))
				{
					neighbourPositions.Add(neighbour);
				}
			}

			foreach (Vector3Ushort neighbour in neighbourPositions)
			{
				if (TryFindRunInColumn(neighbour, out Run neighbourRun) && run != neighbourRun)
				{
					_neighboursByRun[run].Add(neighbourRun);
					_neighboursByRun[neighbourRun].Add(run);
				}
			}
		}

		private void RemoveRun(Run run)
		{
			_columns[run.X * _mapProvider.Map.Depth + run.Z].Remove(run);

			foreach (Run adjacentRun in _neighboursByRun[run])
			{
				_neighboursByRun[adjacentRun].Remove(run);
			}

			_neighboursByRun.Remove(run);
		}

		private IEnumerable<Vector3Ushort> GetPositionsInRun(Run run)
		{
			for (ushort y = run.Begin; y < run.Begin + run.Length; y++)
			{
				var position = new Vector3Ushort(run.X, y, run.Z);
				yield return position;
			}
		}

		private void PreProcessColumns()
		{
			for (ushort x = 0; x < _mapProvider.Map.Width; x++)
			{
				for (ushort z = 0; z < _mapProvider.Map.Depth; z++)
				{
					var runs = new List<Run>();
					ushort startRun = 0;
					ushort length = 0;
					for (ushort y = 0; y < _mapProvider.Map.Height; y++)
					{
						bool isSolid = _mapProvider.Map.GetVoxelByGlobalPosition(x, y, z).IsSolid();
						if (isSolid)
						{
							if (length == 0)
							{
								startRun = y;
							}

							length += 1;
						}

						if (!isSolid || y == _mapProvider.Map.Height - 1)
						{
							if (length > 0)
							{
								runs.Add(new Run(x, z, startRun, length, false));
							}

							length = 0;
						}
					}

					_columns[x * _mapProvider.Map.Depth + z] = runs;
				}
			}
		}

		private void PreProcessGraph()
		{
			for (var i = 0; i < _columns.Length; i++)
			{
				for (var j = 0; j < _columns[i].Count; j++)
				{
					_neighboursByRun[_columns[i][j]] = new HashSet<Run>();
				}
			}

			for (ushort x = 0; x < _mapProvider.Map.Width; x++)
			{
				for (ushort z = 0; z < _mapProvider.Map.Depth; z++)
				{
					for (ushort y = 0; y < _mapProvider.Map.Height; y++)
					{
						Vector3Ushort position = new Vector3Ushort(x, y, z);

						if (!_mapProvider.Map.GetVoxelByGlobalPosition(position).IsSolid() || !TryFindRunInColumn(position, out Run currentRun))
						{
							continue;
						}

						foreach (Vector3Ushort neighbour in GetConnectedNeighbours(position))
						{
							if (TryFindRunInColumn(neighbour, out Run neighbourRun) && currentRun != neighbourRun)
							{
								_neighboursByRun[currentRun].Add(neighbourRun);
								_neighboursByRun[neighbourRun].Add(currentRun);
							}
						}
					}
				}
			}
		}

		private bool TryMergeWithExistingRuns(Voxel voxel)
		{
			var runs = _columns[voxel.Position.x * _mapProvider.Map.Depth + voxel.Position.z];

			for (var i = 0; i < runs.Count; i++)
			{
				if (voxel.Position.y >= runs[i].Begin && voxel.Position.y < runs[i].Begin + runs[i].Length)
				{
					return false;
				}
				
				if (runs[i].Begin + runs[i].Length == voxel.Position.y)
				{
					RemoveRun(runs[i]);
					Run mergedRun = runs[i];
					mergedRun.Length += 1;
					AddRun(mergedRun);
					return true;
				}

				if (runs[i].Begin - 1 == voxel.Position.y)
				{
					RemoveRun(runs[i]);
					Run mergedRun = runs[i];
					mergedRun.Begin -= 1;
					mergedRun.Length += 1;
					AddRun(mergedRun);
					return true;
				}
			}

			return false;
		}

		private void SplitRun(Run run, int separationHeight, out Run firstRun, out Run secondRun)
		{
			firstRun = new Run(run.X, run.Z, run.Begin, (ushort)(separationHeight - run.Begin), run.IsCreatedByPlayer);
			secondRun = new Run(run.X, run.Z, (ushort)(separationHeight + 1), (ushort)(run.Length - 1 - firstRun.Length),
				run.IsCreatedByPlayer);
		}

		private bool TryFindRunInColumn(Vector3Ushort position, out Run run)
		{
			var column = _columns[position.x * _mapProvider.Map.Depth + position.z];

			for (var i = 0; i < column.Count; i++)
			{
				if (column[i].Begin <= position.y && position.y < column[i].Begin + column[i].Length)
				{
					run = column[i];
					return true;
				}
			}

			run = default;
			return false;
		}

		private IEnumerable<Vector3Ushort> GetConnectedNeighbours(Vector3Ushort position)
		{
			for (int xOffset = -1; xOffset <= 1; xOffset++)
			{
				for (int yOffset = -1; yOffset <= 1; yOffset++)
				{
					for (int zOffset = -1; zOffset <= 1; zOffset++)
					{
						if (position.x + xOffset >= _mapProvider.Map.Width ||
						    position.y + yOffset >= _mapProvider.Map.Height ||
						    position.z + zOffset >= _mapProvider.Map.Depth ||
						    position.x + xOffset < 0 ||
						    position.y + yOffset < 0 ||
						    position.z + zOffset < 0)
						{
							continue;
						}

						var neighbour = new Vector3Ushort((ushort)(position.x + xOffset), (ushort)(position.y + yOffset),
							(ushort)(position.z + zOffset));

						if (_mapProvider.Map.GetVoxelByGlobalPosition(neighbour).IsSolid())
						{
							yield return neighbour;
						}
					}
				}
			}
		}

		private IEnumerable<Vector3Ushort> GetConnectedNeighboursWithoutDiagonals(Vector3Ushort position)
		{
			for (int xOffset = -1; xOffset <= 1; xOffset++)
			{
				for (int yOffset = -1; yOffset <= 1; yOffset++)
				{
					for (int zOffset = -1; zOffset <= 1; zOffset++)
					{
						if (Math.Abs(xOffset) + Math.Abs(yOffset) + Math.Abs(zOffset) != 1)
						{
							continue;
						}

						if (position.x + xOffset >= _mapProvider.Map.Width ||
						    position.y + yOffset >= _mapProvider.Map.Height ||
						    position.z + zOffset >= _mapProvider.Map.Depth ||
						    position.x + xOffset < 0 ||
						    position.y + yOffset < 0 ||
						    position.z + zOffset < 0)
						{
							continue;
						}

						var neighbour = new Vector3Ushort((ushort)(position.x + xOffset), (ushort)(position.y + yOffset),
							(ushort)(position.z + zOffset));

						if (_mapProvider.Map.GetVoxelByGlobalPosition(neighbour).IsSolid())
						{
							yield return neighbour;
						}
					}
				}
			}
		}

		private struct Run : IEquatable<Run>
		{
			public readonly bool IsCreatedByPlayer;
			public readonly ushort X;
			public readonly ushort Z;
			public ushort Begin;
			public ushort Length;

			public Run(ushort x, ushort z, ushort begin, ushort length, bool isCreatedByPlayer)
			{
				X = x;
				Z = z;
				Begin = begin;
				Length = length;
				IsCreatedByPlayer = isCreatedByPlayer;
			}

			public bool Equals(Run other)
			{
				return IsCreatedByPlayer == other.IsCreatedByPlayer && X == other.X && Z == other.Z && Begin == other.Begin && Length == other.Length;
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
				return Equals(left, right);
			}

			public static bool operator !=(Run left, Run right)
			{
				return !(left == right);
			}

			public override string ToString()
			{
				return $"RUN (X: {X}, Z: {Z}, BEGIN: {Begin}, LENGTH: {Length})";
			}
		}
	}
}