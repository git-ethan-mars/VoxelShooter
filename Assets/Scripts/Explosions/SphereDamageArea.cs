using System.Collections.Generic;
using MapLogic;
using UnityEngine;

namespace Explosions
{
    public class SphereDamageArea : IDamageArea
    {
        private readonly MapProvider _mapProvider;

        public SphereDamageArea(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
        }

        public List<Vector3Int> GetDamagedBlocks(int radius, Vector3Int targetBlock)
        {
            var blockPositions = new List<Vector3Int>();
            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var z = -radius; z <= radius; z++)
                    {
                        var blockPosition = targetBlock + new Vector3Int(x, y, z);
                        if (_mapProvider.IsDestructiblePosition(blockPosition) &&
                            _mapProvider.GetBlockByGlobalPosition(blockPosition).IsSolid())
                        {
                            var blockData = _mapProvider.GetBlockByGlobalPosition(blockPosition);
                            if (Vector3Int.Distance(blockPosition, targetBlock) <= radius
                                && blockData.IsSolid())
                                blockPositions.Add(blockPosition);
                        }
                    }
                }
            }

            return blockPositions;
        }
    }
}