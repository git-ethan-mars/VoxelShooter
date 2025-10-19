using Cysharp.Threading.Tasks;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class ParticleFactory : IParticleFactory
	{
		private readonly IAssetProvider _assets;

		public ParticleFactory(IAssetProvider assets)
		{
			_assets = assets;
		}

		public ParticleSystem CreateBulletImpact(Vector3 position, Quaternion rotation, Color color)
		{
			var particles = _assets.Instantiate(ParticlePath.BulletImpactPath, position, rotation)
				.GetComponent<ParticleSystem>();
			ParticleSystem.MainModule main = particles.main;
			main.startColor = color;
			DestroyParticleAsync(particles.gameObject, particles.main.startLifetime.constant).Forget();
			return particles;
		}

		public ParticleSystem CreateVoxelDestructionParticle(Vector3 position, Quaternion rotation, Color color)
		{
			var particles = _assets.Instantiate(ParticlePath.VoxelDestructionParticlePath, position, rotation)
				.GetComponent<ParticleSystem>();
			ParticleSystem.MainModule main = particles.main;
			main.startColor = color;
			DestroyParticleAsync(particles.gameObject, particles.main.startLifetime.constant).Forget();
			return particles;
		}

		public ParticleSystem CreateBlood(Vector3 position, Quaternion rotation)
		{
			var blood = _assets.Instantiate(ParticlePath.BloodSprayPath, position, rotation)
				.GetComponent<ParticleSystem>();
			DestroyParticleAsync(blood.gameObject, blood.main.startLifetime.constant).Forget();
			return blood;
		}

		public ParticleSystem CreateRchParticle(Vector3 position, int startSpeed, int burstCount, float radius)
		{
			var rchParticle = _assets.Instantiate(ParticlePath.RchParticlePath, position, Quaternion.identity)
				.GetComponent<ParticleSystem>();
			ParticleSystem.MainModule main = rchParticle.main;
			main.startSpeed = startSpeed;
			var burst = new ParticleSystem.Burst(0f, burstCount, 5, 0.05f)
			{
				probability = 1
			};
			ParticleSystem.ShapeModule shape = rchParticle.shape;
			shape.radius = radius;

			rchParticle.emission.SetBurst(0, burst);
			DestroyParticleAsync(rchParticle.gameObject, rchParticle.main.startLifetime.constant).Forget();
			return rchParticle;
		}

		public ParticleSystem CreateFallingMeshParticle(Transform particleContainer)
		{
			return _assets.Instantiate(ParticlePath.FallingMeshParticlePath, particleContainer)
				.GetComponent<ParticleSystem>();
		}

		private async UniTaskVoid DestroyParticleAsync(GameObject particle, float lifetime)
		{
			await UniTask.WaitForSeconds(lifetime, cancellationToken: particle.GetCancellationTokenOnDestroy());
			if (particle != null)
			{
				Object.Destroy(particle);
			}
		}
	}
}