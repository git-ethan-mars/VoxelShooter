using System;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.MapFeatures;
using R3;
using Reflex.Attributes;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	[SelectionBase]
	public class Tombstone : Entity
	{
		[SerializeField] private new Collider collider;
		[SerializeField] private Bounds localBounds;

		[Header("Explosion settings")]
		[SerializeField] private ExplosionData explosionData;
		[SerializeField] private int particleCount;
		[SerializeField] private int particleSpeed;

		private MapProvider _mapProvider;
		private EntityContainerService _entityContainer;
		private IParticleFactory _particleFactory;

		[Inject]
		private void Construct(MapProvider mapProvider, EntityContainerService entityContainer, IParticleFactory particleFactory)
		{
			_mapProvider = mapProvider;
			_entityContainer = entityContainer;
			_particleFactory = particleFactory;
		}

		private void Start()
		{
			_entityContainer.Add(this);
		}

		private void OnDestroy()
		{
			_entityContainer.Remove(this);
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			_mapProvider.Map.MapUpdated
				.Subscribe(_ => ValidatePosition())
				.AddTo(this);
		}

		public async UniTaskVoid ExplodeWithDelay(TimeSpan delay)
		{
			await UniTask.Delay(delay, cancellationToken: destroyCancellationToken);

			if (destroyCancellationToken.IsCancellationRequested)
			{
				return;
			}

			_particleFactory.CreateRchParticle(transform.position, particleSpeed, particleCount, explosionData.radius);

			if (_mapProvider.Map.TryGetFeature(out MapDestruction mapDestruction))
			{
				mapDestruction.Visit(explosionData, transform.position);
			}

			foreach (IDamageVisitor visitor in _entityContainer.GetEntitiesByType<IDamageVisitor>())
			{
				visitor.Visit(explosionData, transform.position);
			}

			Destroy(gameObject);
		}

		private void ValidatePosition()
		{
			while (_mapProvider.Map.HasIntersection(Bounds))
			{
				transform.position += Vector3.up;
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		public override Bounds Bounds => new Bounds(localBounds.center + transform.position, localBounds.size);
	}
}