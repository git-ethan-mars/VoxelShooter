using System.Collections.Generic;
using MapLogic;
using UnityEngine;

namespace Explosions
{
    public class LineBlockArea : IDamageBlockArea
    {
        private const int LineLength = 3;

        private readonly IMapProvider _mapProvider;

        public LineBlockArea(IMapProvider mapProvider)
        {
            _mapProvider = mapProvider;
        }

        public List<Vector3Int> GetOverlappedBlockPositions(Vector3Int centerBlock)
        {
            var blocks = new List<Vector3Int>();
            for (var i = -LineLength / 2; i <= LineLength / 2; i++)
            {
                var offset = new Vector3Int(0, i, 0);
                if (_mapProvider.IsDestructiblePosition(centerBlock + offset) && _mapProvider
                        .GetBlockByGlobalPosition(centerBlock + offset).IsSolid())
                {
                    blocks.Add(centerBlock + offset);
                }
            }

            return blocks;
        }

        public int CalculateBlockDamage(Vector3Int blockPosition, Vector3Int damageCenter, int damage)
        {
            return damage;
        }
    }
}