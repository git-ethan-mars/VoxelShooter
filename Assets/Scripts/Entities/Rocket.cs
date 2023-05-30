using System;
using Data;
using Explosions;
using Infrastructure.Factory;
using Mirror;
using Networking;
using Networking.Messages;
using Networking.Synchronization;
using UnityEngine;

namespace Entities
{
    public class Rocket : NetworkBehaviour
    {
        private ServerData _serverData;
        private IParticleFactory _particleFactory;
        private IExplosionManager _explosionManager;
        private int _radius;
        private int _damage;
        private int _particlesSpeed;
        private int _particlesCount;
        private NetworkConnectionToClient _owner;
        private bool _isExploded;

        public void Construct(ServerData serverData, RocketLauncherItem rocketData,
            NetworkConnectionToClient owner, IParticleFactory particleFactory)
        {
            _serverData = serverData;
            _particleFactory = particleFactory;
            _radius = rocketData.radius;
            _damage = rocketData.damage;
            _particlesSpeed = rocketData.particlesSpeed;
            _particlesCount = rocketData.particlesCount;
            _owner = owner;
            _explosionManager = new ExplosionManager(serverData, particleFactory, new SphereExplosionArea(serverData));
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!isServer || _isExploded) return;
            _isExploded = true;
            var rocketPosition = new Vector3Int((int) transform.position.x,
                (int) transform.position.y, (int) transform.position.z);

            _explosionManager.DestroyExplosiveWithBlocks(rocketPosition, gameObject, _radius, _particlesSpeed, _particlesCount);

            Collider[] hitColliders = Physics.OverlapSphere(rocketPosition, _radius);
            foreach (var hitCollider in hitColliders)
                _explosionManager.DamagePlayer(hitCollider, rocketPosition, _radius, _damage, 
                    _particlesSpeed, _owner);
        }
    }
}