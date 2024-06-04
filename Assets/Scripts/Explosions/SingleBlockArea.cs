using System.Collections.Generic;
using MapLogic;
using UnityEngine;

namespace Explosions
{
    public class SingleBlockArea : IDamageBlockArea
    {
        private readonly IMapProvider _mapProvider;

        public SingleBlockArea(IMapProvider mapProvider)
        {
            _mapProvider = mapProvider;
        }

        public List<Vector3Int> GetOverlappedBlockPositions(Vector3Int centerBlock)
        {
            var overlappedPosition = new List<Vector3Int>();
            if (_mapProvider.IsDestructiblePosition(centerBlock))
            {
                overlappedPosition.Add(centerBlock);
            }

            return overlappedPosition;
        }

        public int CalculateBlockDamage(Vector3Int blockPosition, Vector3Int damageCenter, int damage)
        {
            return damage;
        }
    }
}