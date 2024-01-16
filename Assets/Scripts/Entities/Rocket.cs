using Data;
using Explosions;
using Infrastructure.Factory;
using Mirror;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Entities
{
    public class Rocket : NetworkBehaviour
    {
        private bool _isExploded;
        private AudioService _audioService;
        private RocketLauncherItem _rocketData;
        private IParticleFactory _particleFactory;
        private ExplosionBehaviour _explosionBehaviour;

        public void Construct(IServer server, RocketLauncherItem rocketData, IParticleFactory particleFactory,
            AudioService audioService)
        {
            _rocketData = rocketData;
            _particleFactory = particleFactory;
            _audioService = audioService;
            _explosionBehaviour =
                new ExplosionBehaviour(server, connectionToClient, rocketData.radius, rocketData.damage);
        }

        public void Explode()
        {
            var rocketPosition = transform.position;
            _explosionBehaviour.Explode(rocketPosition);
            _particleFactory.CreateRchParticle(rocketPosition, _rocketData.particlesSpeed,
                _rocketData.particlesCount);
            NetworkServer.SendToReady(new RchParticleResponse(transform.position, _rocketData.particlesSpeed,
                _rocketData.particlesCount));
            _audioService.SendAudio(_rocketData.explosionSound, rocketPosition);
            NetworkServer.Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isServer || _isExploded)
            {
                return;
            }

            if (collision.gameObject.GetComponentInParent<NetworkIdentity>()?.connectionToClient == connectionToClient)
            {
                return;
            }

            Explode();
            _isExploded = true;
        }
    }
}