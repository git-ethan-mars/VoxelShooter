using System.Collections.Generic;
using UnityEngine;

namespace Explosions
{
    public interface IDamageBlockArea
    {
        List<Vector3Int> GetOverlappedBlockPositions(Vector3Int centerBlock);
        int CalculateBlockDamage(Vector3Int blockPosition, Vector3Int damageCenter, int damage);
    }
}