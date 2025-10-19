using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class ColumnDestructionAlgorithm
	{
		private readonly List<Run>[] _columns;
		private readonly HashSet<Vector3Int> _connectedNeighbours;
		private readonly Map _map;
		private readonly List<Vector3Int> _neighbours;
		private readonly Dictionary<Run, HashSet<Run>> _neighboursByRun;
		private readonly Stack<Run> _pathStack;
		private readonly List<Run> _runsToDelete;

		public ColumnDestructionAlgorithm(Map map)
		{
			_map = map;
			_columns = new List<Run>[map.Width * map.Depth];
			_runsToDelete = new List<Run>();
			_pathStack = new Stack<Run>();
			_connectedNeighbours = new HashSet<Vector3Int>();
			_neighbours = new List<Vector3Int>();
			for (var i = 0; i < _columns.Length; i++)
			{
				_columns[i] = new List<Run>();
			}

			_neighboursByRun = new Dictionary<Run, HashSet<Run>>();

			PreProcessColumns();
			PreProcessGraph();
		}

		public void Add(List<Voxel> voxels)
		{
			for (var i = 0; i < voxels.Count; i++)
			{
				var singleVoxelRun = new Run(voxels[i].Position.x, voxels[i].Position.z, voxels[i].Position.y, 1, true);
				if (!TryMergeRuns(singleVoxelRun,
					    _columns[voxels[i].Position.x * _map.Depth + voxels[i].Position.z]))
				{
					AddRun(singleVoxelRun);
				}
			}
		}

		public List<Voxel> Remove(List<Voxel> removingVoxels)
		{
			_runsToDelete.Clear();
			_pathStack.Clear();
			_connectedNeighbours.Clear();
			for (var i = 0; i < removingVoxels.Count; i++)
			{
				Run run = FindRunInColumn(removingVoxels[i].Position.x, removingVoxels[i].Position.z, removingVoxels[i].Position.y);
				if (removingVoxels[i].Position.y == run.Begin || removingVoxels[i].Position.y == run.Begin + run.Length - 1)
				{
					var newRun = new Run(run.X, run.Z, run.Begin, run.Length, run.IsCreatedByPlayer);
					if (removingVoxels[i].Position.y == newRun.Begin)
					{
						newRun.Begin += 1;
					}

					newRun.Length -= 1;
					RemoveRun(run);
					AddRun(newRun);
				}
				else
				{
					SplitRun(run, removingVoxels[i].Position.y, out Run firstRun, out Run secondRun);
					RemoveRun(run);
					AddRun(firstRun);
					AddRun(secondRun);
				}
			}

			for (var i = 0; i < removingVoxels.Count; i++)
			{
				GetConnectedNeighbours(removingVoxels[i].Position.x, removingVoxels[i].Position.y, removingVoxels[i].Position.z);
				for (var j = 0; j < _neighbours.Count; j++)
				{
					_connectedNeighbours.Add(_neighbours[j]);
				}
			}

			var neighbourRuns =
				_connectedNeighbours.Select(position => FindRunInColumn(position.x, position.z, position.y))
					.ToHashSet();
			while (neighbourRuns.Count > 0)
			{
				var visited = new HashSet<Run>();
				Run neighbourRun = neighbourRuns.First();
				_pathStack.Push(neighbourRun);
				var isSeparatedComponent = true;
				while (_pathStack.Count > 0)
				{
					if (!isSeparatedComponent)
					{
						_pathStack.Clear();
						continue;
					}

					Run element = _pathStack.Pop();
					visited.Add(element);
					if (neighbourRuns.Contains(element))
					{
						neighbourRuns.Remove(element);
					}

					foreach (Run nextRun in _neighboursByRun[element])
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

						_pathStack.Push(nextRun);
					}
				}

				if (isSeparatedComponent)
				{
					_runsToDelete.AddRange(visited);
				}
			}


			var fallingVoxels = new List<Voxel>();
			for (var i = 0; i < _runsToDelete.Count; i++)
			{
				for (int height = _runsToDelete[i].Begin;
				     height < _runsToDelete[i].Begin + _runsToDelete[i].Length;
				     height++)
				{
					var voxelPosition = new Vector3Int(_runsToDelete[i].X, height, _runsToDelete[i].Z);
					VoxelData blockData = _map.GetVoxelByGlobalPosition(voxelPosition);
					fallingVoxels.Add(new Voxel(voxelPosition, blockData));
				}
			}

			return fallingVoxels;
		}

		private void AddRun(Run run)
		{
			_columns[run.X * _map.Depth + run.Z].Add(run);
			_neighboursByRun[run] = new HashSet<Run>();
			for (int y = run.Begin; y < run.Begin + run.Length; y++)
			{
				if (run.IsCreatedByPlayer)
				{
					GetConnectedNeighboursWithoutDiagonals(run.X, y, run.Z);
				}
				else
				{
					GetConnectedNeighbours(run.X, y, run.Z);
				}

				for (var i = 0; i < _neighbours.Count; i++)
				{
					Run neighbourRun =
						FindRunInColumn(_neighbours[i].x, _neighbours[i].z,
							_neighbours[i].y);
					if (neighbourRun != null && run != neighbourRun)
					{
						_neighboursByRun[run].Add(neighbourRun);
						_neighboursByRun[neighbourRun].Add(run);
					}
				}
			}
		}

		private void RemoveRun(Run run)
		{
			_columns[run.X * _map.Depth + run.Z].Remove(run);
			foreach (Run adjacentRun in _neighboursByRun[run])
			{
				_neighboursByRun[adjacentRun].Remove(run);
			}

			_neighboursByRun.Remove(run);
		}

		private void PreProcessColumns()
		{
			for (var x = 0; x < _map.Width; x++)
			{
				for (var z = 0; z < _map.Depth; z++)
				{
					var runs = new List<Run>();
					var startRun = 0;
					var length = 0;
					for (var y = 0; y < _map.Height; y++)
					{
						bool isSolid = _map.GetVoxelByGlobalPosition(x, y, z).IsSolid();
						if (isSolid)
						{
							if (length == 0)
							{
								startRun = y;
							}

							length += 1;
						}

						if (!isSolid || y == _map.Height - 1)
						{
							if (length > 0)
							{
								runs.Add(new Run(x, z, startRun, length, false));
							}

							length = 0;
						}
					}

					_columns[x * _map.Depth + z] = runs;
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

			for (var x = 0; x < _map.Width; x++)
			{
				for (var z = 0; z < _map.Depth; z++)
				{
					for (var y = 0; y < _map.Height; y++)
					{
						if (!_map.GetVoxelByGlobalPosition(x, y, z).IsSolid())
						{
							continue;
						}

						Run currentRun = FindRunInColumn(x, z, y);
						GetConnectedNeighbours(x, y, z);
						for (var i = 0; i < _neighbours.Count; i++)
						{
							Run neighbourRun = FindRunInColumn(
								_neighbours[i].x, _neighbours[i].z,
								_neighbours[i].y);
							if (currentRun != neighbourRun)
							{
								_neighboursByRun[currentRun].Add(neighbourRun);
								_neighboursByRun[neighbourRun].Add(currentRun);
							}
						}
					}
				}
			}
		}

		private bool TryMergeRuns(Run run, List<Run> runs)
		{
			for (var i = 0; i < runs.Count; i++)
			{
				if (runs[i].Begin + runs[i].Length == run.Begin && run.IsCreatedByPlayer == runs[i].IsCreatedByPlayer)
				{
					var mergedRun = new Run(runs[i].X, runs[i].Z, runs[i].Begin, runs[i].Length + run.Length,
						run.IsCreatedByPlayer);
					RemoveRun(runs[i]);
					AddRun(mergedRun);
					return true;
				}

				if (run.Begin + run.Length == runs[i].Begin && run.IsCreatedByPlayer == runs[i].IsCreatedByPlayer)
				{
					var mergedRun = new Run(runs[i].X, runs[i].Z, run.Begin, run.Length + runs[i].Length,
						run.IsCreatedByPlayer);
					RemoveRun(runs[i]);
					AddRun(mergedRun);
					return true;
				}
			}

			return false;
		}

		private void SplitRun(Run run, int separationHeight, out Run firstRun, out Run secondRun)
		{
			firstRun = new Run(run.X, run.Z, run.Begin, separationHeight - run.Begin, run.IsCreatedByPlayer);
			secondRun = new Run(run.X, run.Z, separationHeight + 1, run.Length - 1 - firstRun.Length,
				run.IsCreatedByPlayer);
		}

		private Run FindRunInColumn(int columnX, int columnZ, int height)
		{
			var column = _columns[columnX * _map.Depth + columnZ];
			for (var i = 0; i < column.Count; i++)
			{
				if (column[i].Begin <= height && height < column[i].Begin + column[i].Length)
				{
					return column[i];
				}
			}

			return null;
		}

		private void GetConnectedNeighbours(int x, int y, int z)
		{
			_neighbours.Clear();
			for (int xOffset = -1; xOffset <= 1; xOffset++)
			{
				for (int yOffset = -1; yOffset <= 1; yOffset++)
				{
					for (int zOffset = -1; zOffset <= 1; zOffset++)
					{
						if (xOffset == 0 && yOffset == 0 && zOffset == 0)
						{
							continue;
						}

						if (_map.IsInsideMap(x + xOffset, y + yOffset, z + zOffset) && _map
							    .GetVoxelByGlobalPosition(x + xOffset, y + yOffset, z + zOffset).IsSolid())
						{
							_neighbours.Add(new Vector3Int(x + xOffset, y + yOffset, z + zOffset));
						}
					}
				}
			}
		}

		private void GetConnectedNeighboursWithoutDiagonals(int x, int y, int z)
		{
			_neighbours.Clear();
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

						if (_map.IsInsideMap(x + xOffset, y + yOffset, z + zOffset) && _map
							    .GetVoxelByGlobalPosition(x + xOffset, y + yOffset, z + zOffset).IsSolid())
						{
							_neighbours.Add(new Vector3Int(x + xOffset, y + yOffset, z + zOffset));
						}
					}
				}
			}
		}

		private class Run
		{
			public readonly bool IsCreatedByPlayer;
			public readonly int X;
			public readonly int Z;
			public int Begin;
			public int Length;

			public Run(int x, int z, int begin, int length, bool isCreatedByPlayer)
			{
				X = x;
				Z = z;
				Begin = begin;
				Length = length;
				IsCreatedByPlayer = isCreatedByPlayer;
			}
		}
	}
}