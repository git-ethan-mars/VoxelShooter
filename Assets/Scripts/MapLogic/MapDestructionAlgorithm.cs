using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Mirror;
using Networking;
using Networking.Messages.Responses;
using Optimization;
using UnityEngine;

namespace MapLogic
{
    public class MapDestructionAlgorithm
    {
        private uint componentId;
        private const int EmptyBlockCost = 1000000000;
        private readonly ICoroutineRunner _coroutineRunner;
        public MapProvider MapProvider;
        private MapUpdater _mapUpdater;
        private readonly int _width;
        private readonly int _depth;
        private readonly int _height;
        private const int LowerSolidBlockHeight = 1;
        private const int NeighbourCountToHideBlock = 6;

        private List<Tuple<int, int, int>> _mirrorBlocks = new()
        {
            new Tuple<int, int, int>(1, 1, 1),
            new Tuple<int, int, int>(-1, 1, 1),
            new Tuple<int, int, int>(1, -1, 1),
            new Tuple<int, int, int>(1, 1, -1),
            new Tuple<int, int, int>(-1, -1, 1),
            new Tuple<int, int, int>(-1, 1, -1),
            new Tuple<int, int, int>(1, -1, -1),
            new Tuple<int, int, int>(-1, -1, -1)
        };

        public MapDestructionAlgorithm(ICoroutineRunner coroutineRunner, MapProvider mapProvider, MapUpdater mapUpdater)
        {
            _coroutineRunner = coroutineRunner;
            MapProvider = mapProvider;
            _mapUpdater = mapUpdater;
            _width = mapProvider.MapData.Width;
            _height = mapProvider.MapData.Height;
            _depth = mapProvider.MapData.Depth;

            FindColorfulBlocks();
        }

        private void FindColorfulBlocks()
        {
            for (var x = 0; x < _width; x++)
            {
                for (var y = LowerSolidBlockHeight; y < _height; y++)
                {
                    for (var z = 0; z < _depth; z++)
                    {
                        var currentBlock = y * _width * _width + z * _width + x;
                        var currentBlockData = MapProvider.GetBlockByGlobalPosition(x, y, z);
                        if (currentBlockData.IsSolid())
                        {
                            MapProvider.MapData._solidBlocks.Add(currentBlock);
                            MapProvider.MapData._blockColors.Add(currentBlock, currentBlockData.Color);
                        }
                    }
                }
            }
        }

        public int GetVertexIndex(Vector3Int vertex)
        {
            return vertex.y * _width * _width + vertex.z * _width + vertex.x;
        }

        private (int x, int y, int z) GetVertexCoordinates(int index)
        {
            var y = index >> 18;
            var z = (index - (y << 18)) >> 9;
            var x = index - (y << 18) - (z << 9);
            return (x, y, z);
        }

        private Vector3Int GetVectorByIndex(int index)
        {
            var y = index >> 18;
            var z = (index - (y << 18)) >> 9;
            var x = index - (y << 18) - (z << 9);
            return new Vector3Int(x, y, z);
        }

        private List<int> GetNeighborsByIndex(int index)
        {
            return new List<int>
            {
                index + _width * _width,
                index - _width * _width,
                index + _width,
                index - _width,
                index + 1,
                index - 1
            };
        }

        private List<int> GetNeighborsWithDiagonalByIndex(int index)
        {
            return new List<int>
            {
                index + _width * _width,
                index - _width * _width,
                index + _width,
                index - _width,
                index + 1,
                index - 1,
                index - _width * _width - _width - 1,
                index - _width * _width - _width,
                index - _width * _width - _width + 1,
                index - _width * _width - 1,
                index - _width * _width + 1,
                index - _width * _width + _width - 1,
                index - _width * _width + _width,
                index - _width * _width + _width + 1,
                index - _width - 1,
                index - _width + 1,
                index + _width - 1,
                index + _width + 1,
                index + _width * _width - _width - 1,
                index + _width * _width - _width,
                index + _width * _width - _width + 1,
                index + _width * _width - 1,
                index + _width * _width + 1,
                index + _width * _width + _width - 1,
                index + _width * _width + _width,
                index + _width * _width + _width + 1
            };
        }

        private double Heuristic(Vector3Int target, int neighbour)
        {
            var neighbourCoordinates = GetVertexCoordinates(neighbour);

            return (target.x - neighbourCoordinates.x) * (target.x - neighbourCoordinates.x)
                   + (target.y - neighbourCoordinates.y) * (target.y - neighbourCoordinates.y)
                   + (target.z - neighbourCoordinates.z) * (target.z - neighbourCoordinates.z);
        }

        private void FillQueue(Vector3Int targetVertex, Dictionary<int, double> priceByVertex,
            int vertex, PriorityQueue<int, double> pq, List<int> outerBlocksToDelete)
        {
            var number = 0;
            var neighborsCounter = 0;
            foreach (var neighbour in MapProvider.MapData._blocksPlacedByPlayer.Contains(vertex)
                         ? GetNeighborsByIndex(vertex)
                         : GetNeighborsWithDiagonalByIndex(vertex))
            {
                var cost = !MapProvider.MapData._solidBlocks.Contains(neighbour) ? EmptyBlockCost : 1;
                var newCost = priceByVertex[vertex] + cost;
                if (!priceByVertex.ContainsKey(neighbour) || newCost < priceByVertex[neighbour])
                {
                    if (number < NeighbourCountToHideBlock)
                        neighborsCounter++;
                    priceByVertex[neighbour] = newCost;
                    var priority = newCost + Heuristic(targetVertex, neighbour);
                    pq.Enqueue(neighbour, priority);
                }

                number++;
            }

            if (neighborsCounter < NeighbourCountToHideBlock)
                outerBlocksToDelete.Add(vertex);
        }

        private void AStar(int startVertex, Vector3Int targetVertex,
            List<Tuple<List<int>, List<int>>> isolatedComponents, HashSet<int> globalCheckedBlocks)
        {
            var allBlocksToDelete = new List<int>();
            var outerBlocks = new List<int>() {startVertex};
            var localCheckedBlocks = new HashSet<int>();
            var priceByVertex = new Dictionary<int, double>
            {
                [startVertex] = 0
            };

            var pq = new PriorityQueue<int, double>();
            pq.Enqueue(startVertex, Heuristic(targetVertex, startVertex));
            while (pq.Count > 0)
            {
                var vertex = pq.Dequeue();

                if (priceByVertex[vertex] >= EmptyBlockCost)
                {
                    isolatedComponents.Add(Tuple.Create(allBlocksToDelete, outerBlocks));
                    foreach (var block in allBlocksToDelete)
                        MapProvider.MapData._solidBlocks.Remove(block);
                    globalCheckedBlocks.UnionWith(localCheckedBlocks);
                    return;
                }

                localCheckedBlocks.Add(vertex);

                if (globalCheckedBlocks.Contains(vertex))
                {
                    globalCheckedBlocks.UnionWith(localCheckedBlocks);
                    return;
                }

                if (GetVertexCoordinates(vertex).y == targetVertex.y)
                {
                    globalCheckedBlocks.UnionWith(localCheckedBlocks);
                    return;
                }

                allBlocksToDelete.Add(vertex);

                FillQueue(targetVertex, priceByVertex, vertex, pq, outerBlocks);
            }
        }

        private int GetDistance(int x, int y, int z)
        {
            return x * x + y * y + z * z;
        }

        private void AddBlockToSphere(int index, HashSet<int> sphere)
        {
            if (MapProvider.MapData._solidBlocks.Contains(index))
                sphere.Add(index);
        }

        private (HashSet<int> explosionInnerBlocks, HashSet<int> explosionOuterBlocks) GetExplosionSphere(
            Vector3Int selectedBlock, int radius)
        {
            var explosionInnerBlocks = new HashSet<int>();
            var explosionOuterBlocks = new HashSet<int>();

            var radiusSq = radius * radius;
            var radius1Sq = (radius - 1) * (radius - 1);
            var ceilRadius = radius;
            for (var x = 0; x <= ceilRadius; x++)
            {
                for (var y = 0; y <= ceilRadius; y++)
                {
                    for (var z = 0; z <= ceilRadius; z++)
                    {
                        var dSq = GetDistance(x, y, z);
                        if (dSq > radiusSq)
                            continue;
                        if (dSq < radius1Sq || (GetDistance(x + 1, y, z) <= radiusSq
                                                && GetDistance(x, y + 1, z) <= radiusSq
                                                && GetDistance(x, y, z + 1) <= radiusSq))
                        {
                            foreach (var mirrorBlock in _mirrorBlocks)
                            {
                                AddBlockToSphere((selectedBlock.y + y * mirrorBlock.Item1) * _width * _width
                                                 + (selectedBlock.z + z * mirrorBlock.Item2) * _width
                                                 + (selectedBlock.x + x * mirrorBlock.Item3), explosionInnerBlocks);
                            }
                        }
                        else
                        {
                            foreach (var mirrorBlock in _mirrorBlocks)
                            {
                                AddBlockToSphere((selectedBlock.y + y * mirrorBlock.Item1) * _width * _width
                                                 + (selectedBlock.z + z * mirrorBlock.Item2) * _width
                                                 + (selectedBlock.x + x * mirrorBlock.Item3), explosionOuterBlocks);
                            }
                        }
                    }
                }
            }

            return (explosionInnerBlocks, explosionOuterBlocks);
        }

        private (HashSet<int> explosionInnerBlocks, HashSet<int> explosionOuterBlocks)
            GetExplosionSphereWithSingleRadius(Vector3Int selectedBlock)
        {
            var explosionInnerBlocks = new HashSet<int>();
            var explosionOuterBlocks = new HashSet<int>();
            var targetBlock = GetVertexIndex(selectedBlock);

            explosionInnerBlocks.Add(targetBlock);
            foreach (var block in GetNeighborsWithDiagonalByIndex(targetBlock))
                if (MapProvider.MapData._solidBlocks.Contains(block))
                    explosionOuterBlocks.Add(block);

            return (explosionInnerBlocks, explosionOuterBlocks);
        }

        private void UpdateBlocks(int[] blocks)
        {
            Vector3Int[] blockPositions = new Vector3Int[blocks.Length];

            for (var i = 0; i < blocks.Length; i++)
            {
                MapProvider.MapData._blockColors[blocks[i]] = new Color32();
                blockPositions[i] = GetVectorByIndex(blocks[i]);
                _mapUpdater.SetBlockByGlobalPosition(blockPositions[i], new BlockData());
            }

            var splitter = new BlockSplitter();
            var messages = splitter.SplitBytesIntoMessages(blockPositions, new BlockData[blocks.Length], 100000);
            _coroutineRunner.StartCoroutine(splitter.SendMessages(messages, null, 1));
        }

        private void ExplodeBlocks(HashSet<int> explosionInnerBlocks)
        {
            var explosionBlocksToDelete = new HashSet<int>();

            foreach (var innerBlock in explosionInnerBlocks)
            {
                explosionBlocksToDelete.Add(innerBlock);
                MapProvider.MapData._solidBlocks.Remove(innerBlock);
                MapProvider.MapData._blocksPlacedByPlayer.Remove(innerBlock);
            }

            UpdateBlocks(explosionBlocksToDelete.ToArray());
        }

        private void DestroyIsolatedComponents(HashSet<int> explosionOuterBlocks, Vector3Int block)
        {
            var startVertexes = new List<int>();
            foreach (var index in explosionOuterBlocks)
                if (MapProvider.MapData._solidBlocks.Contains(index))
                    startVertexes.Add(index);

            var targetVertex = new Vector3Int(block.x, LowerSolidBlockHeight, block.z);
            var isolatedComponents = new List<Tuple<List<int>, List<int>>>();
            var globalCheckedBlocks = new HashSet<int>();

            foreach (var startVertex in startVertexes)
                AStar(startVertex, targetVertex, isolatedComponents, globalCheckedBlocks);

            /*foreach (var isolatedComponent in isolatedComponents)
            {
                NetworkServer.SendToAll(new FallBlockResponse(
                    isolatedComponent.Item2.Select(GetVectorByIndex).ToArray(),
                    isolatedComponent.Item2.Select(position => MapProvider.MapData._blockColors[position]).ToArray(),
                    componentId));
                componentId += 1;
            }*/

            if (isolatedComponents.Count > 0)
            {
                var blocksToDelete = isolatedComponents.SelectMany(x => x.Item1).ToArray();
                UpdateBlocks(blocksToDelete);
            }
        }

        public void StartDestruction(Vector3Int block, int radius)
        {
            var (explosionInnerBlocks, explosionOuterBlocks) = radius > 1
                ? GetExplosionSphere(block, radius)
                : GetExplosionSphereWithSingleRadius(block);

            ExplodeBlocks(explosionInnerBlocks);
            DestroyIsolatedComponents(explosionOuterBlocks, block);
        }
    }
}