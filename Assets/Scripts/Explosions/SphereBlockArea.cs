using System.Collections.Generic;
using MapLogic;
using UnityEngine;

namespace Explosions
{
    public class SphereBlockArea : IDamageBlockArea
    {
        private readonly IMapProvider _mapProvider;
        private readonly int _radius;

        public SphereBlockArea(IMapProvider mapProvider, int radius)
        {
            _mapProvider = mapProvider;
            _radius = radius;
        }

        public List<Vector3Int> GetOverlappedBlockPositions(Vector3Int centerBlock)
        {
            var blockPositions = new List<Vector3Int>();
            for (var x = -_radius; x <= _radius; x++)
            {
                for (var y = -_radius; y <= _radius; y++)
                {
                    for (var z = -_radius; z <= _radius; z++)
                    {
                        var blockPosition = centerBlock + new Vector3Int(x, y, z);
                        if (_mapProvider.IsDestructiblePosition(blockPosition) &&
                            _mapProvider.GetBlockByGlobalPosition(blockPosition).IsSolid())
                        {
                            var blockData = _mapProvider.GetBlockByGlobalPosition(blockPosition);
                            if (Vector3.Distance(blockPosition, centerBlock) <= _radius && blockData.IsSolid())
                            {
                                blockPositions.Add(blockPosition);
                            }
                        }
                    }
                }
            }

            return blockPositions;
        }

        public int CalculateBlockDamage(Vector3Int blockPosition, Vector3Int damageCenter, int damage)
        {
            return (int) ((1 - Vector3Int.Distance(blockPosition, damageCenter) / _radius) * damage);
        }
    }
}