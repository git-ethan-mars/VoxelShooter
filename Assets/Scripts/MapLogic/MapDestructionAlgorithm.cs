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
    public class MapDestructionAlgorithm
    {
        private const int EmptyBlockCost = 1000000000;
        public IMapProvider MapProvider;
        private IMapUpdater _mapUpdater;
        private readonly int _width;
        private readonly int _depth;
        private readonly int _height;
        private const int LowerSolidBlockHeight = 1;
        
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

        public MapDestructionAlgorithm(IMapProvider mapProvider, IMapUpdater mapUpdater)
        {
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
                        var currentBlock = x * _width * _width + y * _width + z;
                        var currentBlockData = MapProvider.GetBlockByGlobalPosition(x, y, z);
                        if (currentBlockData.IsSolid())
                            MapProvider.MapData._solidBlocks.Add(currentBlock);
                    }
                }
            }
        }

        public int GetVertexIndex(Vector3Int vertex)
        {
            return vertex.x * _width * _width + vertex.y * _width + vertex.z;
        }
        
        private Vector3Int GetVertexCoordinates(int index)
        {
            var x = index / (_width * _width);
            var y = (index - x * (_width * _width)) / _width;
            var z = index - x * _width * _width - y * _width;
            return new Vector3Int(x, y, z);
        }

        private double Heuristic(Vector3Int target, Vector3Int neighbour)
        {
            return (target.x - neighbour.x) * (target.x - neighbour.x) 
                   + (target.y - neighbour.y) * (target.y - neighbour.y)
                   + (target.z - neighbour.z) * (target.z - neighbour.z);
        }
        
        private void FillQueue(Vector3Int targetVertex, List<Vector3Int> neighbours, 
            Dictionary<Vector3Int, double> priceByVertex, 
            Vector3Int vertex, PriorityQueue<Vector3Int, double> pq)
        {
            foreach (var neighbour in neighbours)
            {
                var cost = !MapProvider.MapData._solidBlocks.Contains(GetVertexIndex(neighbour)) ? EmptyBlockCost : 1;
                var newCost = priceByVertex[vertex] + cost;
                if (!priceByVertex.ContainsKey(neighbour) || newCost < priceByVertex[neighbour])
                {
                    priceByVertex[neighbour] = newCost;
                    var priority = newCost + Heuristic(targetVertex, neighbour);
                    pq.Enqueue(neighbour, priority);
                }
            }
        }

        private void AStar(Vector3Int startVertex, Vector3Int tagretVertex, 
            List<List<Vector3Int>> isolatedComponents, HashSet<Vector3Int> globalCheckedBlocks)
        {
            var blocksToDelete = new List<Vector3Int>();
            var localCheckedBlocks = new HashSet<Vector3Int>();
            var priceByVertex = new Dictionary<Vector3Int, double>();
            
            priceByVertex[startVertex] = 0;
            var pq = new PriorityQueue<Vector3Int, double>();
            pq.Enqueue(startVertex, Heuristic(startVertex, tagretVertex));
            while (pq.Count > 0)
            {
                var vertex = pq.Dequeue();

                if (priceByVertex[vertex] >= EmptyBlockCost)
                {
                    isolatedComponents.Add(blocksToDelete);
                    foreach (var v in blocksToDelete) 
                        MapProvider.MapData._solidBlocks.Remove(GetVertexIndex(v));
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

                FillQueue(tagretVertex, neighbours, priceByVertex, vertex, pq);
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
                MapProvider.MapData._solidBlocks.Remove(innerBlock);
            }

            UpdateBlocks(explosionBlocksToDelete.ToList());
        }
        
        private void DestroyIsolatedComponents(HashSet<int> explosionOuterBlocks, Vector3Int block)
        {
            var startVertexes = new List<Vector3Int>();
            foreach (var index in explosionOuterBlocks)
            {
                if (MapProvider.MapData._solidBlocks.Contains(index))
                    startVertexes.Add(GetVertexCoordinates(index));
            }

            var targetVertex = new Vector3Int(block.x, LowerSolidBlockHeight, block.z);
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

        public void StartDestruction(Vector3Int block, int radius)
        {
            var (explosionInnerBlocks, explosionOuterBlocks) = GetExplosionSphere(block, radius);

            ExplodeBlocks(explosionInnerBlocks);
            DestroyIsolatedComponents(explosionOuterBlocks, block);
        }
    }
}