using System.Collections.Generic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class LinearBlockDamageCalculator : IBlockDamageCalculator
    {
        public List<int> Calculate(Vector3Int targetBlock, int radius, int damage, List<Vector3Int> damagedBlocks)
        {
            List<int> damageList = new List<int>(damagedBlocks.Count);
            foreach (var damagedBlock in damagedBlocks)
            {
                damageList.Add((int)((1 - Vector3Int.Distance(damagedBlock, targetBlock) / radius) * damage));
            }
            return damageList;
        }
    }   
}