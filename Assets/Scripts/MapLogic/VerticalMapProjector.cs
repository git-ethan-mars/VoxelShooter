using System.Collections.Generic;
using Data;
using Networking;
using UnityEngine;

namespace MapLogic
{
    public class VerticalMapProjector
    {
        public readonly Color32[] Projection;
        private readonly IMapProvider _mapProvider;
        
        public VerticalMapProjector(IMapProvider mapProvider)
        {
            _mapProvider = mapProvider;
            Projection = GetVerticalMapProjection();
        }

        public void UpdateProjection(BlockDataWithPosition[] blocks)
        {
            var visitedPositions = new HashSet<(int, int)>();
            foreach (var block in blocks)
            {
                if (visitedPositions.Add((block.Position.x, block.Position.z)))
                {
                    Projection[block.Position.z * _mapProvider.Width + block.Position.x] =
                        GetHighestBlock(block.Position.x, block.Position.z).Color;
                }
            }
        }

        private Color32[] GetVerticalMapProjection()
        {
            var projection = new Color32[_mapProvider.Width * _mapProvider.Depth];
            for (var x = 0; x < _mapProvider.Width; x++)
            {
                for (var z = 0; z < _mapProvider.Depth; z++)
                {
                    projection[z * _mapProvider.Width + x] = GetHighestBlock(x, z).Color;
                }
            }

            return projection;
        }

        private BlockData GetHighestBlock(int x, int z)
        {
            for (var y = _mapProvider.Height - 1; y >= 0; y--)
            {
                var block = _mapProvider.GetBlockByGlobalPosition(x, y, z);
                if (!block.IsSolid())
                {
                    continue;
                }

                return block;
            }

            return new BlockData(BlockColor.empty);
        }
    }
}