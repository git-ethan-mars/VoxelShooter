using Data;
using Explosions;
using Infrastructure.Factory;
using Mirror;
using Networking;
using Networking.ServerServices;

namespace Entities
{
    public class Tnt : NetworkBehaviour, IExplosive
    {
        private IParticleFactory _particleFactory;
        private AudioService _audioService;
        private TntItem _tntItem;
        private ExplosionBehaviour _explosionBehaviour;

        public void Construct(IServer server, NetworkConnectionToClient owner,
            IParticleFactory particleFactory, AudioService audioService, TntItem tntItem)
        {
            _tntItem = tntItem;
            _particleFactory = particleFactory;
            _audioService = audioService;
            _explosionBehaviour = new ExplosionBehaviour(server, owner, tntItem.radius, tntItem.damage);
        }

        public void Explode()
        {
            var tntPosition = transform.position;
            _explosionBehaviour.Explode(tntPosition);
            _particleFactory.CreateRchParticle(tntPosition, _tntItem.particlesSpeed,
                _tntItem.particlesCount);
            _audioService.SendAudio(_tntItem.explosionSound, tntPosition);
            NetworkServer.Destroy(gameObject);
        }
    }
}