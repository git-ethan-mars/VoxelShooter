using System.Collections.Generic;
using MapLogic;
using UnityEngine;

namespace Explosions
{
    public class LineExplosionArea : IExplosionArea
    {
        private readonly IMapProvider _mapProvider;

        public LineExplosionArea(IMapProvider mapProvider)
        {
            _mapProvider = mapProvider;
        }

        public List<Vector3Int> GetExplodedBlocks(int radius, Vector3Int targetBlock)
        {
            var blocks = new List<Vector3Int>();
            for (var i = -radius / 2; i <= radius / 2; i++)
            {
                if (_mapProvider.IsValidPosition(targetBlock + new Vector3Int(0, i, 0)))
                {
                    blocks.Add(targetBlock + new Vector3Int(0, i, 0));
                }
            }

            return blocks;
        }
    }
}