using System.Collections;
using System.Collections.Generic;
using Common.Factory;
using UnityEngine;

namespace Common
{
    public class FallingMeshParticlePool
    {
        private const int PoolSize = 3000;
        private const string ContainerName = "FallingMeshParticleContainer";
        private readonly Stack<ParticleSystem> _stack = new(PoolSize);

        public FallingMeshParticlePool(IParticleFactory particleFactory)
        {
            var particleContainer = new GameObject(ContainerName);
            for (var i = 0; i < PoolSize; i++)
            {
                var meshParticle = particleFactory.CreateFallingMeshParticle(particleContainer.transform);
                meshParticle.gameObject.SetActive(false);
                _stack.Push(meshParticle);
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