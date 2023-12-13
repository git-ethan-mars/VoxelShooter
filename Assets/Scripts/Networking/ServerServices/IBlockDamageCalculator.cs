using UnityEngine;

namespace Networking.ServerServices
{
    public interface IBlockDamageCalculator
    {
        int Calculate(Vector3Int explosionCenter, int radius, int damage, Vector3Int blockPosition);
    }
}