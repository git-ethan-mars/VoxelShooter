using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Explosions
{
    public interface IExplosionManager
    {
        public void DamagePlayer(Collider hitCollider, Vector3 explosionCenter, int radius, int damage,
            int particlesSpeed, NetworkConnectionToClient connection);

        public void DetonateExplosive(Action action, Collider hitCollider, List<GameObject> exploded, string explosiveTag);

        public void DestroyExplosiveWithBlocks(Vector3Int explosionCenter, GameObject explosive, int radius, int particlesSpeed,
            int particlesCount);
    }
}