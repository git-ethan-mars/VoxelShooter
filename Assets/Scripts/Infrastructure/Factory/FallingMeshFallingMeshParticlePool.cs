using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class FallingMeshFallingMeshParticlePool : IFallingMeshParticlePool
    {
        private const int PoolSize = 1000;
        private const string ContainerName = "FallingMeshParticleContainer";
        private readonly Stack<ParticleSystem> _stack = new(PoolSize);

        public FallingMeshFallingMeshParticlePool(IGameFactory gameFactory, IParticleFactory particleFactory)
        {
            var particleContainer = gameFactory.CreateGameObjectContainer(ContainerName).transform;
            for (var i = 0; i < PoolSize; i++)
            {
                var obj = particleFactory.CreateFallingMeshParticle(particleContainer);
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