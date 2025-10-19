using R3;
using Reflex.Attributes;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	public class SpawnPoint : MonoBehaviour
	{
		[SerializeField] private Bounds localBounds;

		private MapProvider _mapProvider;

		[Inject]
		private void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
		}

		private void Start()
		{
			_mapProvider.Map.MapUpdated
				.Subscribe(_ => ValidatePosition())
				.AddTo(this);
		}

		private void ValidatePosition()
		{
			while (!_mapProvider.Map.HasIntersection(Bounds))
			{
				transform.position += Vector3.down;
			}

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

		public Bounds Bounds => new Bounds(localBounds.center + transform.position, localBounds.size);
	}
}