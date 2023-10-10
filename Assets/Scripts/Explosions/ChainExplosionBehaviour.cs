using System.Collections.Generic;
using System.Linq;
using Infrastructure.Factory;
using MapLogic;
using Mirror;
using UnityEngine;

namespace Explosions
{
    public class ChainExplosionBehaviour : ExplosionBehaviour
    {
        public ChainExplosionBehaviour(MapUpdater mapUpdater, IParticleFactory particleFactory, 
            IExplosionArea explosionArea) 
            : base(mapUpdater, particleFactory, explosionArea) { }

        public override void Explode(Vector3Int explosionCenter, GameObject explosive, int radius, 
            NetworkConnectionToClient connection, int damage, int particlesSpeed, 
            int particlesCount, List<GameObject> exploded, string explosiveTag)
        {
            exploded.Add(explosive);
            DestroyExplosiveWithBlocks(explosionCenter, explosive, radius, particlesSpeed, particlesCount);
            Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag(explosiveTag) && exploded.All(x => x.gameObject != hitCollider.gameObject))
                    Explode(Vector3Int.FloorToInt(hitCollider.gameObject.transform.position),
                        hitCollider.gameObject, radius, connection, damage, particlesSpeed,
                        particlesCount, exploded, explosiveTag);
                DamagePlayer(hitCollider, explosionCenter, radius, damage, particlesSpeed, connection);
            }
        }
    }
}