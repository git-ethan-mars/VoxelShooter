using System.Collections.Generic;
using Data;
using Networking;
using UnityEngine;

namespace Explosions
{
    public class SphereExplosionArea : IExplosionArea
    {
        private ServerData _serverData;

        public SphereExplosionArea(ServerData serverData)
        {
            _serverData = serverData;
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
                        if (_serverData.Map.IsValidPosition(blockPosition)) 
                        {
                            var blockData = _serverData.Map.GetBlockByGlobalPosition(blockPosition);
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