using System.Collections.Generic;
using Infrastructure.Factory;
using Mirror;
using Networking;
using UnityEngine;

namespace Explosions
{
    public class SingleExplosionManager : ExplosionManager, IExplosionManager
    {
        public SingleExplosionManager(ServerData serverData, IParticleFactory particleFactory, 
            IExplosionArea explosionArea) : base(serverData, particleFactory, explosionArea) { }
        
        public void Explode(Vector3Int explosionCenter, GameObject explosive, int radius, 
            NetworkConnectionToClient connection, int damage, int particlesSpeed, 
            int particlesCount, List<GameObject> exploded, string explosiveTag)
        {
            DestroyExplosiveWithBlocks(explosionCenter, explosive, radius, particlesSpeed, particlesCount);
            Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, radius);
            foreach (var hitCollider in hitColliders)
                DamagePlayer(hitCollider, explosionCenter, radius, damage, particlesSpeed, connection);
        }
    }
}