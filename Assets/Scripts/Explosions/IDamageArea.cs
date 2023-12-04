using System.Collections.Generic;
using UnityEngine;

namespace Explosions
{
    public interface IDamageArea
    {
        public List<Vector3Int> GetDamagedBlocks(int radius, Vector3Int targetBlock);
    }
}