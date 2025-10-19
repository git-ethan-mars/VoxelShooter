using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace GamePlay.MapFeatures
{
	public class FallingMeshParticlePool
	{
		private const int PoolSize = 3000;
		private const string ContainerName = "FallingMeshParticleContainer";
		private readonly Stack<ParticleSystem> _stack = new Stack<ParticleSystem>(PoolSize);

		public FallingMeshParticlePool(IParticleFactory particleFactory)
		{
			var particleContainer = new GameObject(ContainerName);
			for (var i = 0; i < PoolSize; i++)
			{
				ParticleSystem meshParticle = particleFactory.CreateFallingMeshParticle(particleContainer.transform);
				meshParticle.gameObject.SetActive(false);
				_stack.Push(meshParticle);
			}
		}

		public ParticleSystem Get()
		{
			ParticleSystem obj = _stack.Pop();
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