using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Infrastructure.Factory
{
    public class FallingMeshFallingMeshParticlePool : IFallingMeshParticlePool
    {
        private Stack<ParticleSystem> _stack = new Stack<ParticleSystem>();
        private const int ObjectCount = 10000;
        
        public FallingMeshFallingMeshParticlePool(IParticleFactory particleFactory)
        {
            // _objectPool = new ObjectPool<ParticleSystem>(createFunc: particleFactory.CreateFallingMeshParticle,
            //     actionOnGet: (particleSystem) => particleSystem.gameObject.SetActive(true), 
            //     actionOnRelease: (obj) => obj.gameObject.SetActive(false), 
            //     actionOnDestroy: (obj) => Object.Destroy(obj.gameObject), 
            //     collectionCheck: false, defaultCapacity: ObjectCount, maxSize: ObjectCount);

            // var particleSystems = new List<ParticleSystem>();
            // for (var i = 0; i < ObjectCount; i++)
            // {
            //     particleSystems.Add(_objectPool.Get());
            // }
            //
            // foreach (var VARIABLE in particleSystems)
            // {
            //     _objectPool.Release(VARIABLE);
            // }

            for (var i = 0; i < ObjectCount; i++)
            {
                var obj = particleFactory.CreateFallingMeshParticle();
                obj.gameObject.SetActive(false);
                _stack.Push(obj);
            }
        }

        public ParticleSystem Get()
        {
            var obj = _stack.Pop();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Release(ParticleSystem particleSystem)
        {
            particleSystem.gameObject.SetActive(false);
            _stack.Push(particleSystem);
        }
        
        public IEnumerator ReleaseOnDelay(ParticleSystem particleSystem, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            Release(particleSystem);
        }
    }
}