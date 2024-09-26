using Common.Factory;
using GamePlay.Entities;
using UnityEngine;

namespace Entities
{
	public class Tombstone : Entity, IPushable
	{
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

		public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
		public Vector3Int Min => new(-Size.x / 2, -Size.y / 2, -Size.z / 2);
		public Vector3Int Max => new(Size.x / 2, Size.y / 2, Size.z / 2);

		private Vector3Int Size => Vector3Int.RoundToInt(collider.bounds.size);

		public void Construct(IParticleFactory particleFactory)
		{
			_particleFactory = particleFactory;
			StartCoroutine(Utils.DoActionAfterDelay(Explode, delayInSeconds));
		}

		public void Push()
		{
			transform.position += Vector3.up;
		}

		public void Fall()
		{
		}

		private void Explode()
		{
			var explosionBehaviour = new ExplosionBehaviour(_host, connectionToClient, radius, damage);
			explosionBehaviour.Explode(transform.position);
			_particleFactory.CreateRchParticle(transform.position, particleSpeed, particleCount);
			NetworkServer.SendToReady(new RchParticleResponse(transform.position, particleSpeed, particleCount));
			NetworkServer.Destroy(gameObject);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position, Size);
		}
	}
}