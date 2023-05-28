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
        private int _radius;
        private int _damage;
        private int _particlesSpeed;
        private int _particlesCount;
        private NetworkConnectionToClient _owner;
        private IExplosionArea _sphereExplosionArea;
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
            _sphereExplosionArea = new SphereExplosionArea(serverData);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!isServer || _isExploded) return;
            _isExploded = true;
        
            var rocketPosition = new Vector3Int((int) transform.position.x,
                (int) transform.position.y, (int) transform.position.z);

            var blockPositions = _sphereExplosionArea.GetExplodedBlocks(_radius, rocketPosition);

            foreach (var position in blockPositions)
                _serverData.Map.SetBlockByGlobalPosition(position, new BlockData());
            _particleFactory.CreateRchParticle(rocketPosition, _particlesSpeed, _particlesCount);

            NetworkServer.SendToAll(new UpdateMapMessage(blockPositions.ToArray(),
                new BlockData[blockPositions.Count]));
            NetworkServer.Destroy(gameObject);

            Collider[] hitColliders = Physics.OverlapSphere(rocketPosition, _radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    var playerPosition = hitCollider.transform.position;
                    var distance = Math.Sqrt(
                        (rocketPosition.x - playerPosition.x) * (rocketPosition.x - playerPosition.x) +
                        (rocketPosition.y - playerPosition.y) * (rocketPosition.y - playerPosition.y) +
                        (rocketPosition.z - playerPosition.z) * (rocketPosition.z - playerPosition.z));
                    var currentDamage = (int) (_damage - _damage * (distance / _radius));
                    var direction = playerPosition - rocketPosition;
                    hitCollider.GetComponent<CharacterController>().Move(direction * _particlesSpeed / 3);
                    var receiver = hitCollider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                    receiver.identity.GetComponent<HealthSynchronization>().Damage(_owner, receiver, currentDamage);
                }
            }
        }
    }
}