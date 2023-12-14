using System;
using System.Collections.Generic;
using Infrastructure.Factory;
using Mirror;
using Networking;
using UnityEngine;

namespace Explosions
{
    public abstract class ExplosionBehaviour
    {
        private readonly IServer _server;
        private readonly IDamageArea _damageArea;
        private readonly IParticleFactory _particleFactory;

        public abstract void Explode(Vector3Int explosionCenter, GameObject explosive, int radius,
            NetworkConnectionToClient connection, int damage, int particlesSpeed,
            int particlesCount, List<GameObject> exploded, string explosiveTag);

        protected ExplosionBehaviour(IServer server, IParticleFactory particleFactory,
            IDamageArea damageArea)
        {
            _server = server;
            _damageArea = damageArea;
            _particleFactory = particleFactory;
        }

        protected void DamagePlayer(GameObject damagedPlayer, Vector3 explosionCenter, int radius, int damage,
            NetworkConnectionToClient source)
        {
            var playerPosition = damagedPlayer.transform.position;
            var distance = Math.Sqrt(
                (explosionCenter.x - playerPosition.x) * (explosionCenter.x - playerPosition.x) +
                (explosionCenter.y - playerPosition.y) * (explosionCenter.y - playerPosition.y) +
                (explosionCenter.z - playerPosition.z) * (explosionCenter.z - playerPosition.z));
            if (distance >= radius)
                distance = radius;
            var currentDamage = (int) (damage - damage * (distance / radius));
            var receiver = damagedPlayer.GetComponentInParent<NetworkIdentity>().connectionToClient;
            _server.Damage(source, receiver, currentDamage);
        }

        protected void DestroyExplosiveWithBlocks(Vector3Int explosionCenter, GameObject explosive, int radius,
            int particlesSpeed, int particlesCount, int damage)
        {
            var rchParticle = _particleFactory.CreateRchParticle(explosionCenter, particlesSpeed, particlesCount);
            NetworkServer.Spawn(rchParticle);
            NetworkServer.Destroy(explosive);
            _server.BlockHealthSystem.DamageBlock(explosionCenter, radius, damage, _damageArea);
        }
    }
}