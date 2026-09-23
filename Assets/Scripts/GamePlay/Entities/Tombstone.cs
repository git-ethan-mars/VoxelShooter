using System;
using Cysharp.Threading.Tasks;
using Data;
using R3;
using Reflex.Attributes;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	[SelectionBase]
	public class Tombstone : Explosive
	{
		[SerializeField] private new Collider collider;
		[SerializeField] private Bounds localBounds;

		[Header("Explosion settings")]
		[SerializeField] private ExplosionData explosionData;
		[SerializeField] private int particleCount;
		[SerializeField] private int particleSpeed;

		private IParticleFactory _particleFactory;

		[Inject]
		private void Construct(MapProvider mapProvider, EntityContainer entityContainer, IParticleFactory particleFactory)
		{
			MapProvider = mapProvider;
			EntityContainer = entityContainer;
			_particleFactory = particleFactory;
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			MapProvider.Map.CurrentValue.MapUpdated
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
			
			Explode(explosionData);

			_particleFactory.CreateRchParticle(transform.position, particleSpeed, particleCount, explosionData.radius);

			Destroy(gameObject);
		}

		private void ValidatePosition()
		{
			while (MapProvider.Map.CurrentValue.HasIntersection(Bounds))
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
		public override ExplosiveType Type => ExplosiveType.Tombstone;
	}
}