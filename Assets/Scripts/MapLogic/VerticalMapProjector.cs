using System.Collections.Generic;
using Data;
using Networking;
using UnityEngine;

namespace MapLogic
{
    public class VerticalMapProjector
    {
        public readonly Color32[] Projection;
        private readonly IClient _client;

        public VerticalMapProjector(IClient client)
        {
            _client = client;
            Projection = GetVerticalMapProjection();
            _client.MapUpdated += OnMapUpdated;
        }

        public void Dispose()
        {
            _client.MapUpdated -= OnMapUpdated;
        }

        private Color32[] GetVerticalMapProjection()
        {
            var projection = new Color32[_client.MapProvider.Width * _client.MapProvider.Depth];
            for (var x = 0; x < _client.MapProvider.Width; x++)
            {
                for (var z = 0; z < _client.MapProvider.Depth; z++)
                {
                    projection[z * _client.MapProvider.Width + x] = GetHighestBlock(x, z).Color;
                }
            }

            return projection;
        }

        private void OnMapUpdated(BlockDataWithPosition[] blocks)
        {
            var visitedPositions = new HashSet<(int, int)>();
            foreach (var block in blocks)
            {
                if (visitedPositions.Add((block.Position.x, block.Position.z)))
                {
                    Projection[block.Position.z * _client.MapProvider.Width + block.Position.x] =
                        GetHighestBlock(block.Position.x, block.Position.z).Color;
                }
            }
        }
        
        private BlockData GetHighestBlock(int x, int z)
        {
            for (var y = _client.MapProvider.Height - 1; y >= 0; y--)
            {
                var block = _client.MapProvider.GetBlockByGlobalPosition(x, y, z);
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