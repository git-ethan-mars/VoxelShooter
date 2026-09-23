using Data;
using Mirror;
using Reflex.Attributes;
using UnityEngine;
namespace GamePlay
{
	public class Dummy : Entity
	{
		[SerializeField] private Bounds localBounds;
		
		private IParticleFactory _particleFactory;
		private HealthSystem _healthSystem;
		
		[SyncVar] private GameClass _gameClass;

		[Inject]
		private void Construct(EntityContainer entityContainer, IParticleFactory particleFactory)
		{
			EntityContainer = entityContainer;
			_particleFactory = particleFactory;
		}
		
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		public override Bounds Bounds => new Bounds(transform.position + localBounds.center, localBounds.size);
	}
}