using Data;
using Explosions;
using Infrastructure.Factory;
using Mirror;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Entities
{
    public class Tnt : NetworkBehaviour, IExplosive
    {
        public Vector3 Position => transform.position;
        private IParticleFactory _particleFactory;
        private AudioService _audioService;
        private TntItem _tntItem;
        private ExplosionBehaviour _explosionBehaviour;
        private IServer _server;
        private bool _isExploded;
        private Vector3Int _linkedPosition;


        public void Construct(IServer server, NetworkConnectionToClient owner,
            IParticleFactory particleFactory, AudioService audioService, TntItem tntItem, Vector3Int linkedPosition)
        {
            _server = server;
            _server.MapUpdater.MapUpdated += OnMapUpdated;
            _tntItem = tntItem;
            _particleFactory = particleFactory;
            _audioService = audioService;
            _linkedPosition = linkedPosition;
            _explosionBehaviour = new ExplosionBehaviour(server, owner, tntItem.radius, tntItem.damage);
        }

        public void Explode()
        {
            if (_isExploded)
            {
                return;
            }

            _isExploded = true;
            var tntPosition = transform.position;
            _explosionBehaviour.Explode(tntPosition);
            _particleFactory.CreateRchParticle(tntPosition, _tntItem.particlesSpeed,
                _tntItem.particlesCount);
            _audioService.SendAudio(_tntItem.explosionSound, tntPosition);
            _server.EntityContainer.RemoveExplosive(this);
            NetworkServer.Destroy(gameObject);
        }

        private void OnMapUpdated()
        {
            if (!_server.MapProvider.GetBlockByGlobalPosition(_linkedPosition).IsSolid() && !_isExploded)
            {
                Explode();
            }
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                _server.MapUpdater.MapUpdated -= OnMapUpdated;
            }
        }
    }
}