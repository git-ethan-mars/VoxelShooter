using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GamePlay
{
	public class FallingMeshParticlePool
	{
		private const string ContainerName = "FallingMeshParticleContainer";

		private readonly Stack<ParticleSystem> _stack = new Stack<ParticleSystem>();
		private readonly IParticleFactory _particleFactory;

		private Transform _container;

		public FallingMeshParticlePool(IParticleFactory particleFactory)
		{
			_particleFactory = particleFactory;
		}

		// Particles are created on demand and reused afterwards.
		public ParticleSystem Get()
		{
			if (_container == null)
			{
				_container = new GameObject(ContainerName).transform;
				_stack.Clear();
			}

			ParticleSystem particles = _stack.Count > 0 ? _stack.Pop() : _particleFactory.CreateFallingMeshParticle(_container);
			particles.gameObject.SetActive(true);
			return particles;
		}

		public async UniTask ReleaseAsync(ParticleSystem particleSystem, float lifetime)
		{
			await UniTask.WaitForSeconds(lifetime);

			if (particleSystem == null)
			{
				return;
			}

			particleSystem.gameObject.SetActive(false);
			_stack.Push(particleSystem);
		}
	}
}
