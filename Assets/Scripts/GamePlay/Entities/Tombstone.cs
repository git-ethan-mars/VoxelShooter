using GamePlay.MapFeatures;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Entities
{
	public class Tombstone : Entity, IPushable
	{
		[SerializeField] 
		private ExplosionData explosion;
		
		[SerializeField]
		private new Collider collider;

		[Header("Explosion settings")]
		[SerializeField]
		private int radius;

		[SerializeField]
		private int damage;

		[SerializeField]
		private float delayInSeconds;

		[SerializeField]
		private int particleCount;

		[SerializeField]
		private int particleSpeed;
		private MapProvider _mapProvider;

		public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
		public Vector3Int Min => new(-Size.x / 2, -Size.y / 2, -Size.z / 2);
		public Vector3Int Max => new(Size.x / 2, Size.y / 2, Size.z / 2);

		private Vector3Int Size => Vector3Int.RoundToInt(collider.bounds.size);

		public void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
		}
		
		public void Push()
		{
			transform.position += Vector3.up;
		}

		public void Fall()
		{
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position, Size);
		}
	}
}