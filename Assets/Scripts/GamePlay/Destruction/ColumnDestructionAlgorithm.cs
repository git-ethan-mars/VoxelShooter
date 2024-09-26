using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Destruction
{
	public class ColumnDestructionAlgorithm
	{
		private readonly MapProvider _mapProvider;
		private readonly List<Run>[] _columns;
		private readonly Dictionary<Run, HashSet<Run>> _neighboursByRun;
		private readonly List<Run> _runsToDelete;
		private readonly Stack<Run> _pathStack;
		private readonly HashSet<Vector3Int> _connectedNeighbours;
		private readonly List<Vector3Int> _neighbours;

		public ColumnDestructionAlgorithm(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
			_columns = new List<Run>[mapProvider.Width * mapProvider.Depth];
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

		public void Add(List<BlockDataWithPosition> blocks)
		{
			for (var i = 0; i < blocks.Count; i++)
			{
				var singleVoxelRun = new Run(blocks[i].Position.x, blocks[i].Position.z, blocks[i].Position.y, 1, true);
				if (!TryMergeRuns(singleVoxelRun,
					_columns[blocks[i].Position.x * _mapProvider.Depth + blocks[i].Position.z]))
				{
					AddRun(singleVoxelRun);
				}
			}
		}

		public List<BlockDataWithPosition> Remove(List<BlockDataWithPosition> removingBlocks)
		{
			_runsToDelete.Clear();
			_pathStack.Clear();
			_connectedNeighbours.Clear();
			for (var i = 0; i < removingBlocks.Count; i++)
			{
				var run = FindRunInColumn(removingBlocks[i].Position.x, removingBlocks[i].Position.z, removingBlocks[i].Position.y);
				if (removingBlocks[i].Position.y == run.Begin || removingBlocks[i].Position.y == run.Begin + run.Length - 1)
				{
					var newRun = new Run(run.X, run.Z, run.Begin, run.Length, run.IsCreatedByPlayer);
					if (removingBlocks[i].Position.y == newRun.Begin)
					{
						newRun.Begin += 1;
					}

					newRun.Length -= 1;
					RemoveRun(run);
					AddRun(newRun);
				}
				else
				{
					SplitRun(run, removingBlocks[i].Position.y, out var firstRun, out var secondRun);
					RemoveRun(run);
					AddRun(firstRun);
					AddRun(secondRun);
				}
			}

			for (var i = 0; i < removingBlocks.Count; i++)
			{
				GetConnectedNeighbours(removingBlocks[i].Position.x, removingBlocks[i].Position.y, removingBlocks[i].Position.z);
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
				var neighbourRun = neighbourRuns.First();
				_pathStack.Push(neighbourRun);
				var isSeparatedComponent = true;
				while (_pathStack.Count > 0)
				{
					if (!isSeparatedComponent)
					{
						_pathStack.Clear();
						continue;
					}

					var element = _pathStack.Pop();
					visited.Add(element);
					if (neighbourRuns.Contains(element))
					{
						neighbourRuns.Remove(element);
					}

					foreach (var nextRun in _neighboursByRun[element])
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


			var fallingBlocks = new List<BlockDataWithPosition>();
			for (var i = 0; i < _runsToDelete.Count; i++)
			{
				for (var height = _runsToDelete[i].Begin;
					height < _runsToDelete[i].Begin + _runsToDelete[i].Length;
					height++)
				{
					var blockPosition = new Vector3Int(_runsToDelete[i].X, height, _runsToDelete[i].Z);
					var blockData = _mapProvider.GetBlockByGlobalPosition(blockPosition); 
					fallingBlocks.Add(new BlockDataWithPosition(blockPosition, blockData));
				}
			}

			return fallingBlocks;
		}

		private void AddRun(Run run)
		{
			_columns[run.X * _mapProvider.Depth + run.Z].Add(run);
			_neighboursByRun[run] = new HashSet<Run>();
			for (var y = run.Begin; y < run.Begin + run.Length; y++)
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
					var neighbourRun =
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
			_columns[run.X * _mapProvider.Depth + run.Z].Remove(run);
			foreach (var adjacentRun in _neighboursByRun[run])
			{
				_neighboursByRun[adjacentRun].Remove(run);
			}

			_neighboursByRun.Remove(run);
		}

		private void PreProcessColumns()
		{
			for (var x = 0; x < _mapProvider.Width; x++)
			{
				for (var z = 0; z < _mapProvider.Depth; z++)
				{
					var runs = new List<Run>();
					var startRun = 0;
					var length = 0;
					for (var y = 0; y < _mapProvider.Height; y++)
					{
						var isSolid = _mapProvider.GetBlockByGlobalPosition(x, y, z).IsSolid();
						if (isSolid)
						{
							if (length == 0)
							{
								startRun = y;
							}

							length += 1;
						}

						if (!isSolid || y == _mapProvider.Height - 1)
						{
							if (length > 0)
							{
								runs.Add(new Run(x, z, startRun, length, false));
							}

							length = 0;
						}
					}

					_columns[x * _mapProvider.Depth + z] = runs;
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

			for (var x = 0; x < _mapProvider.Width; x++)
			{
				for (var z = 0; z < _mapProvider.Depth; z++)
				{
					for (var y = 0; y < _mapProvider.Height; y++)
					{
						if (!_mapProvider.GetBlockByGlobalPosition(x, y, z).IsSolid())
						{
							continue;
						}

						var currentRun = FindRunInColumn(x, z, y);
						GetConnectedNeighbours(x, y, z);
						for (var i = 0; i < _neighbours.Count; i++)
						{
							var neighbourRun = FindRunInColumn(
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
			var column = _columns[columnX * _mapProvider.Depth + columnZ];
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
			for (var xOffset = -1; xOffset <= 1; xOffset++)
			{
				for (var yOffset = -1; yOffset <= 1; yOffset++)
				{
					for (var zOffset = -1; zOffset <= 1; zOffset++)
					{
						if (xOffset == 0 && yOffset == 0 && zOffset == 0)
						{
							continue;
						}

						if (_mapProvider.IsInsideMap(x + xOffset, y + yOffset, z + zOffset) && _mapProvider
							.GetBlockByGlobalPosition(x + xOffset, y + yOffset, z + zOffset).IsSolid())
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
			for (var xOffset = -1; xOffset <= 1; xOffset++)
			{
				for (var yOffset = -1; yOffset <= 1; yOffset++)
				{
					for (var zOffset = -1; zOffset <= 1; zOffset++)
					{
						if (Math.Abs(xOffset) + Math.Abs(yOffset) + Math.Abs(zOffset) != 1)
						{
							continue;
						}

						if (_mapProvider.IsInsideMap(x + xOffset, y + yOffset, z + zOffset) && _mapProvider
							.GetBlockByGlobalPosition(x + xOffset, y + yOffset, z + zOffset).IsSolid())
						{
							_neighbours.Add(new Vector3Int(x + xOffset, y + yOffset, z + zOffset));
						}
					}
				}
			}
		}

		private class Run
		{
			public readonly int X;
			public readonly int Z;
			public int Begin;
			public int Length;
			public readonly bool IsCreatedByPlayer;

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