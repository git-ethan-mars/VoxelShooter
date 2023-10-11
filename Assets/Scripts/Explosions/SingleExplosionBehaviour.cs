using System.Collections.Generic;
using Infrastructure.Factory;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using UnityEngine;

namespace Explosions
{
    public class SingleExplosionBehaviour : ExplosionBehaviour
    {
        public SingleExplosionBehaviour(MapUpdater mapUpdater, IParticleFactory particleFactory, 
            IExplosionArea explosionArea, MapDestructionAlgorithm mapDestructionAlgorithm) : base(mapUpdater, particleFactory, explosionArea, mapDestructionAlgorithm) { }
        
        public override void Explode(Vector3Int explosionCenter, GameObject explosive, int radius, 
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