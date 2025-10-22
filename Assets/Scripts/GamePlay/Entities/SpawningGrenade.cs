using Reflex.Attributes;
using UnityEngine;
namespace GamePlay
{
	public class SpawningGrenade : Entity
	{
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private BoxCollider boxCollider;
		private EntityContainerService _entityContainer;

		[Inject]
		private void Construct(EntityContainerService entityContainer)
		{
			_entityContainer = entityContainer;
		}

		private void Start()
		{
			_entityContainer.Add(this);
		}

		private void OnDestroy()
		{
			_entityContainer.Remove(this);
		}

		public void Throw(Vector3 direction, float throwForce)
		{
			rigidBody.AddForce(direction * throwForce);
		}
		
		/*[ServerCallback]
		public async UniTask ExplodeAsync(CancellationToken cancellationToken)
		{
			canvas.transform.position = gameObject.transform.position + new Vector3(0, 1.5f, 0);

			int elapsedTime = _configure.DelayInSeconds;
			timerText.SetText(elapsedTime.ToString());

			while (elapsedTime > 0 && !cancellationToken.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);

				elapsedTime -= 1;
				timerText.SetText(elapsedTime.ToString());
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			Explode();
		}

		private void Explode()
		{
			if (_mapProvider.Map.TryGetMapFeature(out MapDestruction mapDestruction))
			{
				mapDestruction.Visit(_configure.ExplosionData, transform.position);
			}	

			foreach (IDamageVisitor visitor in _entityContainer.GetEntitiesByType<IDamageVisitor>())
			{
				visitor.Visit(_configure.ExplosionData, transform.position);
			}

			_particleFactory.CreateRchParticle(transform.position, _configure.ParticleSpeed, _configure.ParticleCount,
				_configure.ExplosionData.radius);
			_audioPlayer.Play(explosionAudio, transform.position);
			
			_disposable?.Dispose();

			Destroy(gameObject);
		}*/

		public override Bounds Bounds => boxCollider.bounds;
	}
}