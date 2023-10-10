using System.Collections.Generic;
using MapLogic;
using UnityEngine;

namespace Explosions
{
    public class LineExplosionArea : IExplosionArea
    {
        private readonly MapProvider _mapProvider;

        public LineExplosionArea(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
        }

        public List<Vector3Int> GetExplodedBlocks(int radius, Vector3Int targetBlock)
        {
            var blocks = new List<Vector3Int>();
            for (var i = -radius / 2; i <= radius / 2; i++)
            {
                if (_mapProvider.IsDestructiblePosition(targetBlock + new Vector3Int(0, i, 0)) && _mapProvider
                        .GetBlockByGlobalPosition(targetBlock + new Vector3Int(0, i, 0)).IsSolid())
                {
                    blocks.Add(targetBlock + new Vector3Int(0, i, 0));
                }
            }

            return blocks;
        }
    }
}