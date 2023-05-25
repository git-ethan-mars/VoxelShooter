using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace Explosions
{
    public class LineExplosionArea : IExplosionArea
    {
        private ServerData _serverData;

        public LineExplosionArea(ServerData serverData)
        {
            _serverData = serverData;
        }
        
        public List<Vector3Int> GetExplodedBlocks(int radius, Vector3Int targetBlock)
        {
            var blocks = new List<Vector3Int>();
            for (var i = - radius / 2; i <= radius / 2; i++)
            {
                if (_serverData.Map.IsValidPosition(targetBlock + new Vector3Int(0, i, 0)))
                {
                    blocks.Add(targetBlock + new Vector3Int(0, i, 0));
                }
            }

            return blocks;
        }
    }
}