using Data;
using GamePlay.MapFeatures;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	public class Rocket : Entity
	{
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private BoxCollider boxCollider;

		private MapProvider _mapProvider;
		private IParticleFactory _particleFactory;
		private EntityContainerService _entityContainer;
		private RocketLauncherConfigure _configure;

		[Inject]
		private void Construct(MapProvider mapProvider, IParticleFactory particleFactory, IStaticDataService staticData,
			EntityContainerService entityContainer)
		{
			_mapProvider = mapProvider;
			_particleFactory = particleFactory;
			_entityContainer = entityContainer;
			_configure = staticData.GetItemConfigure<RocketLauncherConfigure>(ItemType.RocketLauncher);
		}

		private void Start()
		{
			_entityContainer.Add(this);
		}

		private void OnDestroy()
		{
			_entityContainer.Remove(this);
		}

		public void Launch()
		{
			rigidBody.linearVelocity = transform.forward * _configure.Speed;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (_mapProvider.Map.TryGetFeature(out MapDestruction mapDestruction))
			{
				mapDestruction.Visit(_configure.ExplosionData, transform.position);
			}

			foreach (IDamageVisitor visitor in _entityContainer.GetEntitiesByType<IDamageVisitor>())
			{
				visitor.Visit(_configure.ExplosionData, transform.position);
			}

			_particleFactory.CreateRchParticle(transform.position, _configure.ParticleSpeed, _configure.ParticleCount, _configure.ExplosionData.radius);
			Destroy(gameObject);
		}

		public override Bounds Bounds => boxCollider.bounds;
	}
}