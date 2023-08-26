using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Mirror;
using Networking.Messages;
using Optimization;
using UnityEngine;

namespace MapLogic
{
    public class Algorithm
    {
        private HashSet<int> _solidBlocks = new();
        private const int MapSize = 512;
        private const int EmptyBlockCost = 1000000000;
        private IMapProvider _mapProvider;
        private IMapUpdater _mapUpdater;
        
        private List<Vector3Int> _neighbourVector6 = new()
        {
            new (-1, 0, 0), new (1, 0, 0), 
            new (0, -1, 0), new (0, 1, 0),
            new (0, 0, -1), new (0, 0, 1) 
        };

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

        public Algorithm(IMapProvider mapProvider, IMapUpdater mapUpdater)
        {
            _mapProvider = mapProvider;
            _mapUpdater = mapUpdater;

            FindColorfulBlocks();
        }
        
        private void FindColorfulBlocks()
        {
            for (var x = 2; x < 510; x++)
            {
                for (var y = 2; y < 126; y++)
                {
                    for (var z = 2; z < 510; z++)
                    {
                        var currentBlock = y * MapSize * MapSize + z * MapSize + x;
                        var currentBlockData = _mapProvider.GetBlockByGlobalPosition(x, y, z);
                        if (IsSolid(currentBlockData)) 
                            _solidBlocks.Add(currentBlock);
                    }
                }
            }
        }
        
        private bool IsSolid(BlockData currentBlockData)
        {
            return !(currentBlockData.Color.a == BlockColor.empty.a &&
                     currentBlockData.Color.r == BlockColor.empty.r &&
                     currentBlockData.Color.g == BlockColor.empty.g &&
                     currentBlockData.Color.b == BlockColor.empty.b);
        }

        private int GetVertexIndex(Vector3Int vertex)
        {
            return vertex.y * MapSize * MapSize + vertex.z * MapSize + vertex.x;
        }
        
        private Vector3Int GetVertexCoordinates(int index)
        {
            var y = index / (MapSize * MapSize);
            var z = (index - y * (MapSize * MapSize)) / MapSize;
            var x = index - y * MapSize * MapSize - z * MapSize;
            return new Vector3Int(x, y, z);
        }

        private double Heuristic(Vector3Int target, Vector3Int neighbour)
        {
            return (target.x - neighbour.x) * (target.x - neighbour.x) 
                   + (target.y - neighbour.y) * (target.y - neighbour.y)
                   + (target.z - neighbour.z) * (target.z - neighbour.z);
        }
        
        private void FillQueue(Vector3Int tagretVertex, List<Vector3Int> neighbours, Dictionary<Vector3Int, double> D, 
            Vector3Int vertex, PriorityQueue<Vector3Int, double> pq)
        {
            foreach (var neighbour in neighbours)
            {
                var cost = !_solidBlocks.Contains(GetVertexIndex(neighbour)) ? EmptyBlockCost : 1;
                var newCost = D[vertex] + cost;
                if (!D.ContainsKey(neighbour) || newCost < D[neighbour])
                {
                    D[neighbour] = newCost;
                    var priority = newCost + Heuristic(tagretVertex, neighbour);
                    pq.Enqueue(neighbour, priority);
                }
            }
        }

        private void AStar(Vector3Int startVertex, Vector3Int tagretVertex, 
            List<List<Vector3Int>> isolatedComponents, HashSet<Vector3Int> globalCheckedBlocks)
        {
            var blocksToDelete = new List<Vector3Int>();
            var localCheckedBlocks = new HashSet<Vector3Int>();
            var D = new Dictionary<Vector3Int, double>();
            
            D[startVertex] = 0;
            var pq = new PriorityQueue<Vector3Int, double>();
            pq.Enqueue(startVertex, Heuristic(startVertex, tagretVertex));
            while (pq.Count > 0)
            {
                var vertex = pq.Dequeue();

                if (D[vertex] >= EmptyBlockCost)
                {
                    isolatedComponents.Add(blocksToDelete);
                    foreach (var v in blocksToDelete) 
                        _solidBlocks.Remove(GetVertexIndex(v));
                    globalCheckedBlocks.UnionWith(localCheckedBlocks);
                    return;
                }
                
                blocksToDelete.Add(vertex);
                localCheckedBlocks.Add(vertex);
                
                if (globalCheckedBlocks.Contains(vertex))
                {
                    globalCheckedBlocks.UnionWith(localCheckedBlocks);
                    return;
                }
                
                if (vertex.y == tagretVertex.y)
                {
                    globalCheckedBlocks.UnionWith(localCheckedBlocks);
                    return;
                }

                var neighbours = new List<Vector3Int>();
                foreach (var vector in _neighbourVector6)
                    neighbours.Add(new Vector3Int(vector.x + vertex.x, vector.y + vertex.y, vector.z + vertex.z));

                FillQueue(tagretVertex, neighbours, D, vertex, pq);
            }
        }

        private int GetDistance(int x, int y, int z)
        {
            return x * x + y * y + z * z;
        }

        private void AddBlockToSphere(int index, HashSet<int> sphere)
        {
            if (_solidBlocks.Contains(index)) 
                sphere.Add(index);
        }

        public (HashSet<int> explosionInnerBlocks, HashSet<int> explosionOuterBlocks) GetExplosionSphere(Vector3Int selectedBlock, int radius)
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
                                AddBlockToSphere((selectedBlock.y + y * mirrorBlock.Item1) * MapSize * MapSize 
                                                 + (selectedBlock.z + z * mirrorBlock.Item2) * MapSize 
                                                 + (selectedBlock.x + x * mirrorBlock.Item3), explosionInnerBlocks);
                            }
                        }
                        else
                        {
                            foreach (var mirrorBlock in _mirrorBlocks)
                            {
                                AddBlockToSphere((selectedBlock.y + y * mirrorBlock.Item1) * MapSize * MapSize 
                                                 + (selectedBlock.z + z * mirrorBlock.Item2) * MapSize 
                                                 + (selectedBlock.x + x * mirrorBlock.Item3), explosionOuterBlocks);
                            }
                        }
                    }
                }
            }
            
            return (explosionInnerBlocks, explosionOuterBlocks);
        }

        private void UpdateBlocks(List<Vector3Int> blocks)
        {
            foreach (var position in blocks)
                _mapUpdater.SetBlockByGlobalPosition(position, new BlockData());
            NetworkServer.SendToAll(new UpdateMapMessage(blocks.ToArray(),
                new BlockData[blocks.Count]));
        }
        
        private void ExplodeBlocks(HashSet<int> explosionInnerBlocks)
        {
            var explosionBlocksToDelete = new HashSet<Vector3Int>();

            foreach (var innerBlock in explosionInnerBlocks)
            {
                explosionBlocksToDelete.Add(GetVertexCoordinates(innerBlock));
                _solidBlocks.Remove(innerBlock);
            }

            UpdateBlocks(explosionBlocksToDelete.ToList());
        }
        
        private void DestroyIsolatedComponents(HashSet<int> explosionOuterBlocks, Vector3Int block)
        {
            var startVertexes = new List<Vector3Int>();
            foreach (var index in explosionOuterBlocks)
            {
                if (_solidBlocks.Contains(index))
                    startVertexes.Add(GetVertexCoordinates(index));
            }

            var targetVertex = new Vector3Int(block.x, 2, block.z);
            var isolatedComponents = new List<List<Vector3Int>>();
            var globalCheckedBlocks = new HashSet<Vector3Int>();

            foreach (var startVertex in startVertexes)
                AStar(startVertex, targetVertex, isolatedComponents, globalCheckedBlocks);

            if (isolatedComponents.Count > 0)
            {
                var blocksToDelete = isolatedComponents.SelectMany(x => x).ToList();

                UpdateBlocks(blocksToDelete);
            }
        }

        public void Start(Vector3Int block, int radius)
        {
            var (explosionInnerBlocks, explosionOuterBlocks) = GetExplosionSphere(block, radius);

            ExplodeBlocks(explosionInnerBlocks);
            DestroyIsolatedComponents(explosionOuterBlocks, block);
        }
    }
}