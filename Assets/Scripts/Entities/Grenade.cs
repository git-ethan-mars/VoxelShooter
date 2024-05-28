using Data;
using Explosions;
using Infrastructure.Factory;
using Mirror;
using Networking;
using Networking.Messages.Responses;
using Networking.ServerServices;

namespace Entities
{
    public class Grenade : NetworkBehaviour
    {
        private IParticleFactory _particleFactory;
        private AudioService _audioService;
        private GrenadeItem _grenadeItem;
        private ExplosionBehaviour _explosionBehaviour;

        public void Construct(IServer server,
            IParticleFactory particleFactory, AudioService audioService, GrenadeItem grenadeItem)
        {
            _particleFactory = particleFactory;
            _audioService = audioService;
            _grenadeItem = grenadeItem;
            _explosionBehaviour = new ExplosionBehaviour(server, connectionToClient, grenadeItem.radius, grenadeItem.damage);
        }

        public void Explode()
        {
            var grenadePosition = transform.position;
            _explosionBehaviour.Explode(grenadePosition);
            _particleFactory.CreateRchParticle(grenadePosition, _grenadeItem.particlesSpeed,
                _grenadeItem.particlesCount);
            NetworkServer.SendToReady(new RchParticleResponse(transform.position, _grenadeItem.particlesSpeed,_grenadeItem.particlesCount));
            _audioService.SendAudio(_grenadeItem.explosionSound, grenadePosition);
            NetworkServer.Destroy(gameObject);
        }
    }
}