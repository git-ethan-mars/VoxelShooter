using System;
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

        public void Construct(MapProvider mapProvider, MapUpdater mapUpdater, DrillItem rocketData,
            NetworkConnectionToClient owner, IParticleFactory particleFactory)
        {
            _radius = rocketData.radius;
            _damage = rocketData.damage;
            _owner = owner;
            _explosionBehaviour = new SingleExplosionBehaviour(mapUpdater, particleFactory, new SphereExplosionArea(mapProvider));
        }

        // void Update()
        // {
        //
        //     // The step size is equal to speed times frame time.
        //     float singleStep = 10 * Time.deltaTime;
        //
        //     // Rotate the forward vector towards the target direction by one step
        //     Vector3 newDirection = Vector3.RotateTowards(transform.forward, transform.forward * 10, singleStep, 0.0f);
        //     
        //     // Calculate a rotation a step closer to the target and applies rotation to this object
        //     transform.rotation = Quaternion.LookRotation(newDirection);
        // }
        
        void Update()
        {
            Debug.Log(transform.forward.normalized);
            transform.Rotate(transform.forward.normalized, 30 * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isServer || _isExploded) return;
            _isExploded = true;
            var position = transform.position;
            var rocketPosition = new Vector3Int((int) position.x,
                (int) position.y, (int) position.z);

            // var explodedRockets = new List<GameObject>();
            // _explosionBehaviour.Explode(rocketPosition, gameObject, _radius,_owner, _damage, 
            //     _particlesSpeed, _particlesCount, explodedRockets, gameObject.tag);
        }
    }
}