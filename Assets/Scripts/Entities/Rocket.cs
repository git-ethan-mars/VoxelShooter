using System.Collections.Generic;
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
        private ExplosionBehaviour _explosionBehaviour;
        private NetworkConnectionToClient _owner;
        private bool _isExploded;
        private AudioService _audioService;
        private RocketLauncherItem _rocketData;

        public void Construct(IServer server, RocketLauncherItem rocketData,
            NetworkConnectionToClient owner, IParticleFactory particleFactory, AudioService audioService)
        {
            _owner = owner;
            _explosionBehaviour =
                new SingleExplosionBehaviour(server, particleFactory, new SphereDamageArea(server.MapProvider));
            _rocketData = rocketData;
            _audioService = audioService;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isServer || _isExploded) return;
            if (collision.gameObject.GetComponentInParent<NetworkIdentity>()?.connectionToClient == _owner)
                return;

            _isExploded = true;
            var position = transform.position;
            var rocketPosition = new Vector3Int((int) position.x,
                (int) position.y, (int) position.z);

            var explodedRockets = new List<GameObject>();
            _explosionBehaviour.Explode(rocketPosition, gameObject, _rocketData.radius, _owner, _rocketData.damage,
                _rocketData.particlesSpeed, _rocketData.particlesCount, explodedRockets, gameObject.tag);
            _audioService.SendAudio(_rocketData.explosionSound, rocketPosition);
        }
    }
}