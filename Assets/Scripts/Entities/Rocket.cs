using System.Collections.Generic;
using Data;
using Explosions;
using Infrastructure.Factory;
using MapLogic;
using Mirror;
using UnityEngine;

namespace Entities
{
    public class Rocket : NetworkBehaviour
    {
        private ExplosionBehaviour _explosionBehaviour;
        private int _radius;
        private int _damage;
        private int _particlesSpeed;
        private int _particlesCount;
        private NetworkConnectionToClient _owner;
        private bool _isExploded;

        public void Construct(MapProvider mapProvider, MapUpdater mapUpdater, RocketLauncherItem rocketData,
            NetworkConnectionToClient owner, IParticleFactory particleFactory)
        {
            _radius = rocketData.radius;
            _damage = rocketData.damage;
            _particlesSpeed = rocketData.particlesSpeed;
            _particlesCount = rocketData.particlesCount;
            _owner = owner;
            _explosionBehaviour = new SingleExplosionBehaviour(mapUpdater, particleFactory, new SphereExplosionArea(mapProvider));
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!isServer || _isExploded) return;
            _isExploded = true;
            var position = transform.position;
            var rocketPosition = new Vector3Int((int) position.x,
                (int) position.y, (int) position.z);

            var explodedRockets = new List<GameObject>();
            _explosionBehaviour.Explode(rocketPosition, gameObject, _radius,_owner, _damage, 
                _particlesSpeed, _particlesCount, explodedRockets, gameObject.tag);
        }
    }
}