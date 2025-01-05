using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Factory;
using UnityEngine;

namespace GamePlay.MapFeatures
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

        public async UniTask ReleaseAsync(ParticleSystem particleSystem, float lifetime)
        {
            await UniTask.WaitForSeconds(lifetime);
            particleSystem.gameObject.SetActive(false);
            _stack.Push(particleSystem);
        }
    }
}