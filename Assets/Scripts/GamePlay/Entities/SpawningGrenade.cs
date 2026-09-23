using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
using Reflex.Attributes;
using Services;
using TMPro;
using UnityEngine;
using VoxelMap;
using AudioType = Data.AudioType;

namespace GamePlay
{
	public class SpawningGrenade : Explosive
	{
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private BoxCollider boxCollider;
		[SerializeField] private Canvas canvas;
		[SerializeField] private TextMeshProUGUI timerText;

		private GrenadeConfigure _configure;
		private IParticleFactory _particleFactory;
		private NetworkAudioSender _audioSender;

		public override Bounds Bounds => boxCollider.bounds;
		public override ExplosiveType Type => ExplosiveType.Grenade;

		[Inject]
		private void Construct(EntityContainer entityContainer, MapProvider mapProvider, IStaticDataService staticData,
			IParticleFactory particleFactory, NetworkAudioSender audioSender)
		{
			EntityContainer = entityContainer;
			MapProvider = mapProvider;
			_configure = staticData.GetItemConfigure<GrenadeConfigure>(ItemType.Grenade);
			_particleFactory = particleFactory;
			_audioSender = audioSender;
		}

		public void Throw(Vector3 direction, float throwForce)
		{
			rigidBody.AddForce(direction * throwForce);
		}

		[ServerCallback]
		public async UniTask ExplodeAsync(CancellationToken cancellationToken)
		{
			canvas.transform.position = gameObject.transform.position + new Vector3(0, 1.5f, 0);

			float elapsedTime = _configure.DelayInSeconds;
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

			ExplodeWithFx();
		}

		private void ExplodeWithFx()
		{
			Explode(_configure.ExplosionData);

			_particleFactory.CreateRchParticle(transform.position, _configure.ParticleSpeed, _configure.ParticleCount,
				_configure.ExplosionData.radius);
			_audioSender.SendAudio(AudioType.GrenadeExplosion, transform.position);

			Destroy(gameObject);
		}
	}
}
