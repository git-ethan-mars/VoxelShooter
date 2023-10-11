using System.Collections.Generic;
using Data;
using MapLogic;
using UnityEngine;

namespace Explosions
{
    public class SphereExplosionArea : IExplosionArea
    {
        private readonly MapProvider _mapProvider;

        public SphereExplosionArea(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
        }

        public List<Vector3Int> GetExplodedBlocks(int radius, Vector3Int targetBlock)
        {
            var blockPositions = new List<Vector3Int>();
            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var z = -radius; z <= radius; z++)
                    {
                        var blockPosition = targetBlock + new Vector3Int(x, y, z);
                        if (_mapProvider.IsValidPosition(blockPosition))
                        {
                            var blockData = _mapProvider.GetBlockByGlobalPosition(blockPosition);
                            if (Vector3Int.Distance(blockPosition, targetBlock) <= radius
                                && !blockData.Color.Equals(BlockColor.empty))
                                blockPositions.Add(blockPosition);
                        }
                    }
                }
            }

            return blockPositions;
        }
    }
}