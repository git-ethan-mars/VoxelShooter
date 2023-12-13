using System.Collections.Generic;
using Infrastructure.Factory;
using Mirror;
using Networking;
using UnityEngine;

namespace Explosions
{
    public class SingleExplosionBehaviour : ExplosionBehaviour
    {
        public SingleExplosionBehaviour(IServer server, IParticleFactory particleFactory,
            IDamageArea damageArea) : base(server, particleFactory, damageArea)
        {
        }

        public override void Explode(Vector3Int explosionCenter, GameObject explosive, int radius,
            NetworkConnectionToClient connection, int damage, int particlesSpeed,
            int particlesCount, List<GameObject> exploded, string explosiveTag)
        {
            DestroyExplosiveWithBlocks(explosionCenter, explosive, radius, particlesSpeed, particlesCount, damage);
            Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    DamagePlayer(hitCollider.gameObject, explosionCenter, radius, damage, connection);
                }
            }
        }
    }
}