using R3;
using Reflex.Attributes;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class SpawnPoint : Entity
	{
		[SerializeField] private Bounds localBounds;

		private MapProvider _mapProvider;

		public override Bounds Bounds => new Bounds(localBounds.center + transform.position, localBounds.size);

		[Inject]
		private void Construct(MapProvider mapProvider, EntityContainer entityContainer)
		{
			_mapProvider = mapProvider;
			EntityContainer = entityContainer;
		}

		private void Start()
		{
			_mapProvider.Map.CurrentValue.MapUpdated
				.Subscribe(_ => ValidatePosition())
				.AddTo(this);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		private void ValidatePosition()
		{
			while (!_mapProvider.Map.CurrentValue.HasIntersection(Bounds))
			{
				transform.position += Vector3.down;
			}

			while (_mapProvider.Map.CurrentValue.HasIntersection(Bounds))
			{
				transform.position += Vector3.up;
			}
		}
	}
}
