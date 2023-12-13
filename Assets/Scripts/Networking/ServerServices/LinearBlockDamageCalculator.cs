using UnityEngine;

namespace Networking.ServerServices
{
    public class LinearBlockDamageCalculator : IBlockDamageCalculator
    {
        public int Calculate(Vector3Int explosionCenter, int radius, int damage, Vector3Int blockPosition)
        {
            return (int)((1 - Vector3Int.Distance(blockPosition, explosionCenter) / radius) * damage);
        }
    }   
}