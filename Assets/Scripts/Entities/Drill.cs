using System.Collections.Generic;
using Data;
using Explosions;
using Infrastructure.Factory;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using UnityEngine;

namespace Entities
{
    public class Drill : NetworkBehaviour
    {
        private ExplosionBehaviour _explosionBehaviour;
        private int _radius;
        private int _damage;
        private NetworkConnectionToClient _owner;
        private bool _isExploded;
        private Rigidbody _rigidbody;
        private int _rotationSpeed;
        private int _particlesCount;
        private int _particlesSpeed;
        public int collisionCount;

        public void Construct(MapProvider mapProvider, MapUpdater mapUpdater, DrillItem drillData,
            NetworkConnectionToClient owner, IParticleFactory particleFactory)
        {
            _radius = drillData.radius;
            _damage = drillData.damage;
            _owner = owner;
            _rotationSpeed = drillData.rotationSpeed;
            _particlesCount = drillData.particlesCount;
            _particlesSpeed = drillData.particlesSpeed;
            _explosionBehaviour = new SingleExplosionBehaviour(mapUpdater, particleFactory, new SphereExplosionArea(mapProvider));
            _rigidbody = gameObject.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(Vector3.down);
            var previousZAngle = _rigidbody.rotation.eulerAngles.z;
            _rigidbody.rotation = Quaternion.LookRotation(_rigidbody.velocity) 
                                  * Quaternion.Euler(new Vector3(0, 0, _rotationSpeed + previousZAngle));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;

            collisionCount++;

            var position = transform.position;
            var drillPosition = new Vector3Int((int) position.x,
                (int) position.y, (int) position.z);

            var explodedRockets = new List<GameObject>();
            _explosionBehaviour.Explode(drillPosition, gameObject, _radius, _owner, _damage, 
                _particlesSpeed, _particlesCount, explodedRockets, gameObject.tag);
        }
    }
}