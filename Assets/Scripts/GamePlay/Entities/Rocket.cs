using Data;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class Rocket : Explosive
	{
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private BoxCollider boxCollider;

		private RocketLauncherConfigure _configure;
		private IParticleFactory _particleFactory;
		public override ExplosiveType Type => ExplosiveType.Rocket;

		public override Bounds Bounds => boxCollider.bounds;

		[Inject]
		private void Construct(MapProvider mapProvider, IParticleFactory particleFactory, IStaticDataService staticData,
			EntityContainer entityContainer)
		{
			MapProvider = mapProvider;
			EntityContainer = entityContainer;
			_particleFactory = particleFactory;
			_configure = staticData.GetItemConfigure<RocketLauncherConfigure>(ItemType.RocketLauncher);
		}

		private void OnCollisionEnter(Collision collision)
		{
			Explode(_configure.ExplosionData);

			_particleFactory.CreateRchParticle(transform.position, _configure.ParticleSpeed, _configure.ParticleCount,
				_configure.ExplosionData.radius);
			Destroy(gameObject);
		}

		public void Launch()
		{
			rigidBody.linearVelocity = transform.forward * _configure.Speed;
		}
	}
}
