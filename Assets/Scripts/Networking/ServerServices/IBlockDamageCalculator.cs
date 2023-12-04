using System.Collections.Generic;
using UnityEngine;

namespace Networking.ServerServices
{
    public interface IBlockDamageCalculator
    {
        List<int> Calculate(Vector3Int targetBlock, int radius, int damage, List<Vector3Int> damagedBlocks);
    }
}