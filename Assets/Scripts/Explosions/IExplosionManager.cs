using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Explosions
{
    public interface IExplosionManager
    {
        public void Explode(Vector3Int explosionCenter, GameObject explosive, int radius, 
            NetworkConnectionToClient connection, int damage, int particlesSpeed, 
            int particlesCount, List<GameObject> exploded, string explosiveTag);
    }
}