using System.Collections.Generic;
using UnityEngine;

namespace Explosions
{
    public interface IExplosionArea
    {
        public List<Vector3Int> GetExplodedBlocks(int radius, Vector3Int targetBlock);
        
        public List<Color32> GetExplodedBlockColors(int radius, Vector3Int targetBlock);
    }
}