using System;
using System.Collections.Generic;
using Infrastructure.Factory;
using Mirror;
using Networking.ServerServices;
using Networking.Synchronization;
using UnityEngine;

namespace Explosions
{
    public abstract class ExplosionBehaviour
    {
        private readonly IExplosionArea _explosionArea;

        private readonly IParticleFactory _particleFactory;

        private readonly MapUpdater _mapUpdater;

        public abstract void Explode(Vector3Int explosionCenter, GameObject explosive, int radius,
            NetworkConnectionToClient connection, int damage, int particlesSpeed,
            int particlesCount, List<GameObject> exploded, string explosiveTag);

        protected ExplosionBehaviour(MapUpdater mapUpdater, IParticleFactory particleFactory,
            IExplosionArea explosionArea)
        {
            _explosionArea = explosionArea;
            _mapUpdater = mapUpdater;
            _particleFactory = particleFactory;
        }

        protected void DamagePlayer(Collider hitCollider, Vector3 explosionCenter, int radius, int damage,
            int particlesSpeed, NetworkConnectionToClient connection)
        {
            if (hitCollider.CompareTag("Player"))
            {
                var playerPosition = hitCollider.transform.position;
                var distance = Math.Sqrt(
                    (explosionCenter.x - playerPosition.x) * (explosionCenter.x - playerPosition.x) +
                    (explosionCenter.y - playerPosition.y) * (explosionCenter.y - playerPosition.y) +
                    (explosionCenter.z - playerPosition.z) * (explosionCenter.z - playerPosition.z));
                if (distance >= radius)
                    distance = radius;
                var currentDamage = (int) (damage - damage * (distance / radius));
                var direction = playerPosition - explosionCenter;
                hitCollider.GetComponent<CharacterController>().Move(direction * particlesSpeed / 3);
                var receiver = hitCollider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                if (receiver.identity.TryGetComponent<HealthSynchronization>(out var health))
                    health.Damage(connection, receiver, currentDamage);
            }
        }

        protected void DestroyExplosiveWithBlocks(Vector3Int explosionCenter, GameObject explosive, int radius,
            int particlesSpeed, int particlesCount, string explosiveTag)
        {
            _particleFactory.CreateRchParticle(explosionCenter, particlesSpeed, particlesCount);
            if (explosiveTag != "Drill")
                NetworkServer.Destroy(explosive);
            _mapUpdater.DestroyBlocks(_explosionArea.GetExplodedBlocks(radius, explosionCenter));
        }
    }
}