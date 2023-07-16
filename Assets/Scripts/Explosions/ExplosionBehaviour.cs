using System;
using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Mirror;
using Networking.Messages;
using Networking.Synchronization;
using UnityEngine;

namespace Explosions
{
    public abstract class ExplosionBehaviour
    {
        private readonly IExplosionArea _explosionArea;
        private readonly IParticleFactory _particleFactory;
        private readonly IMapUpdater _mapUpdater;

        protected ExplosionBehaviour(IMapUpdater mapUpdater, IParticleFactory particleFactory, 
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
            int particlesSpeed, int particlesCount)
        {
            var blockPositions = _explosionArea.GetExplodedBlocks(radius, explosionCenter);
            foreach (var position in blockPositions)
                _mapUpdater.SetBlockByGlobalPosition(position, new BlockData());
            _particleFactory.CreateRchParticle(explosionCenter, particlesSpeed, particlesCount);
            NetworkServer.SendToAll(new UpdateMapMessage(blockPositions.ToArray(),
                new BlockData[blockPositions.Count]));
            NetworkServer.Destroy(explosive);
        }
        
        public abstract void Explode(Vector3Int explosionCenter, GameObject explosive, int radius, 
            NetworkConnectionToClient connection, int damage, int particlesSpeed, 
            int particlesCount, List<GameObject> exploded, string explosiveTag);
        
    }
}