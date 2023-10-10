using System;
using System.Collections.Generic;
using System.Linq;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class ColumnDestructionAlgorithm
    {
        private readonly MapProvider _mapProvider;
        private readonly List<Run>[] _columns;
        private readonly Dictionary<Run, HashSet<Run>> _neighboursByRun;

        public ColumnDestructionAlgorithm(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
            _columns = new List<Run>[mapProvider.MapData.Width * mapProvider.MapData.Depth];
            for (var i = 0; i < _columns.Length; i++)
            {
                _columns[i] = new List<Run>();
            }

            _neighboursByRun = new Dictionary<Run, HashSet<Run>>();

            PreProcessColumns();
            PreProcessGraph();
        }

        public void Add(List<Vector3Int> blockPositions)
        {
            for (var i = 0; i < blockPositions.Count; i++)
            {
                var singleVoxelRun = new Run(blockPositions[i].x, blockPositions[i].z, blockPositions[i].y, 1, true);
                if (!TryMergeRuns(singleVoxelRun,
                        _columns[blockPositions[i].x * _mapProvider.MapData.Depth + blockPositions[i].z]))
                {
                    AddRun(singleVoxelRun);
                }
            }
        }

        public Vector3Int[] Remove(List<Vector3Int> removingPositions)
        {
            var runsToDelete = new List<Run>();
            for (var i = 0; i < removingPositions.Count; i++)
            {
                var run = FindRunInColumn(removingPositions[i].x, removingPositions[i].z, removingPositions[i].y);
                if (removingPositions[i].y == run.Begin || removingPositions[i].y == run.Begin + run.Length - 1)
                {
                    var newRun = new Run(run.X, run.Z, run.Begin, run.Length, run.IsCreatedByPlayer);
                    if (removingPositions[i].y == newRun.Begin)
                    {
                        newRun.Begin += 1;
                    }

                    newRun.Length -= 1;
                    RemoveRun(run);
                    AddRun(newRun);
                }
                else
                {
                    SplitRun(run, removingPositions[i].y, out var firstRun, out var secondRun);
                    RemoveRun(run);
                    AddRun(firstRun);
                    AddRun(secondRun);
                }

                var stack = new Stack<Run>();
                var neighbourRuns =
                    GetConnectedNeighbours(removingPositions[i].x, removingPositions[i].y, removingPositions[i].z)
                        .Select(position => FindRunInColumn(position.x, position.z, position.y)).ToHashSet();
                while (neighbourRuns.Count > 0)
                {
                    var visited = new HashSet<Run>();
                    var neighbourRun = neighbourRuns.First();
                    stack.Push(neighbourRun);
                    var isSeparatedComponent = true;
                    while (stack.Count > 0)
                    {
                        if (!isSeparatedComponent)
                        {
                            stack.Clear();
                            continue;
                        }

                        var element = stack.Pop();
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

                            stack.Push(nextRun);
                        }
                    }

                    if (isSeparatedComponent)
                    {
                        runsToDelete.AddRange(visited);
                    }
                }
            }

            var fallingPositions = new List<Vector3Int>();
            for (var i = 0; i < runsToDelete.Count; i++)
            {
                for (var height = runsToDelete[i].Begin;
                     height < runsToDelete[i].Begin + runsToDelete[i].Length;
                     height++)
                {
                    fallingPositions.Add(new Vector3Int(runsToDelete[i].X, height, runsToDelete[i].Z));
                }
            }

            return fallingPositions.ToArray();
        }

        private void AddRun(Run run)
        {
            _columns[run.X * _mapProvider.MapData.Depth + run.Z].Add(run);
            _neighboursByRun[run] = new HashSet<Run>();
            for (var y = run.Begin; y < run.Begin + run.Length; y++)
            {
                var neighbours = run.IsCreatedByPlayer
                    ? GetConnectedNeighboursWithoutDiagonals(run.X, y, run.Z)
                    : GetConnectedNeighbours(run.X, y, run.Z);

                for (var i = 0; i < neighbours.Count; i++)
                {
                    var neighbourRun =
                        FindRunInColumn(neighbours[i].x, neighbours[i].z,
                            neighbours[i].y);
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
            _columns[run.X * _mapProvider.MapData.Depth + run.Z].Remove(run);
            foreach (var adjacentRun in _neighboursByRun[run])
            {
                _neighboursByRun[adjacentRun].Remove(run);
            }

            _neighboursByRun.Remove(run);
        }

        private void PreProcessColumns()
        {
            for (var x = 0; x < _mapProvider.MapData.Width; x++)
            {
                for (var z = 0; z < _mapProvider.MapData.Depth; z++)
                {
                    var runs = new List<Run>();
                    var startRun = 0;
                    var length = 0;
                    for (var y = 0; y < _mapProvider.MapData.Height; y++)
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

                        if (!isSolid || y == _mapProvider.MapData.Height - 1)
                        {
                            if (length > 0)
                            {
                                runs.Add(new Run(x, z, startRun, length, false));
                            }

                            length = 0;
                        }
                    }

                    _columns[x * _mapProvider.MapData.Depth + z] = runs;
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

            for (var x = 0; x < _mapProvider.MapData.Width; x++)
            {
                for (var z = 0; z < _mapProvider.MapData.Depth; z++)
                {
                    for (var y = 0; y < _mapProvider.MapData.Height; y++)
                    {
                        if (!_mapProvider.GetBlockByGlobalPosition(x, y, z).IsSolid())
                        {
                            continue;
                        }

                        var currentRun = FindRunInColumn(x, z, y);
                        var neighbours = GetConnectedNeighbours(x, y, z);
                        for (var i = 0; i < neighbours.Count; i++)
                        {
                            var neighbourRun = FindRunInColumn(
                                neighbours[i].x, neighbours[i].z,
                                neighbours[i].y);
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
            var column = _columns[columnX * _mapProvider.MapData.Depth + columnZ];
            for (var i = 0; i < column.Count; i++)
            {
                if (column[i].Begin <= height && height < column[i].Begin + column[i].Length)
                {
                    return column[i];
                }
            }

            return null;
        }

        private List<Vector3Int> GetConnectedNeighbours(int x, int y, int z)
        {
            var neighbours = new List<Vector3Int>();
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
                            neighbours.Add(new Vector3Int(x + xOffset, y + yOffset, z + zOffset));
                        }
                    }
                }
            }

            return neighbours;
        }

        private List<Vector3Int> GetConnectedNeighboursWithoutDiagonals(int x, int y, int z)
        {
            var neighbours = new List<Vector3Int>();
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
                            neighbours.Add(new Vector3Int(x + xOffset, y + yOffset, z + zOffset));
                        }
                    }
                }
            }

            return neighbours;
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