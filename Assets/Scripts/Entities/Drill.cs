using System.Collections;
using System.Collections.Generic;
using Data;
using Explosions;
using Infrastructure;
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
        private int _lifetime;
        private NetworkConnectionToClient _owner;
        private bool _isExploded;
        private ICoroutineRunner _coroutineRunner;

        public void Construct(MapProvider mapProvider, MapUpdater mapUpdater, DrillItem drillData,
            NetworkConnectionToClient owner, IParticleFactory particleFactory, ICoroutineRunner coroutineRunner)
        {
            _radius = drillData.radius;
            _damage = drillData.damage;
            _lifetime = drillData.lifetime;
            _owner = owner;
            _explosionBehaviour = new SingleExplosionBehaviour(mapUpdater, particleFactory, new SphereExplosionArea(mapProvider));
            _coroutineRunner = coroutineRunner;
            _coroutineRunner.StartCoroutine(DestroyDrill(_lifetime));
        }

        void FixedUpdate()
        {
            transform.Rotate(new Vector3(0, 0, transform.forward.normalized.z), 50 * Time.deltaTime);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;

            var position = transform.position;
            var drillPosition = new Vector3Int((int) position.x,
                (int) position.y, (int) position.z);

            var explodedRockets = new List<GameObject>();
            _explosionBehaviour.Explode(drillPosition, gameObject, _radius, _owner, _damage, 
                0, 0, explodedRockets, gameObject.tag);
        }

        private IEnumerator DestroyDrill(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            NetworkServer.Destroy(gameObject);
        }
    }
}