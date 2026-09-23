using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
using R3;
using Reflex.Attributes;
using Services;
using TMPro;
using UnityEngine;
using VoxelMap;
using AudioType = Data.AudioType;

namespace GamePlay
{
	public class SpawningTNT : Explosive
	{
		[SerializeField] private Bounds localBounds;

		[SerializeField] private Canvas canvas;
		[SerializeField] private TextMeshProUGUI timerText;

		private NetworkAudioSender _audioSender;
		private IParticleFactory _particleFactory;
		private TNTConfigure _configure;

		public override Bounds Bounds => new Bounds(transform.position + localBounds.center, localBounds.size);
		public override ExplosiveType Type => ExplosiveType.Tnt;

		[Inject]
		private void Construct(IStaticDataService staticData, MapProvider mapProvider, EntityContainer entityContainer,
			NetworkAudioSender audioSender, IParticleFactory particleFactory)
		{
			MapProvider = mapProvider;
			EntityContainer = entityContainer;
			_configure = staticData.GetItemConfigure<TNTConfigure>(ItemType.TNT);
			_audioSender = audioSender;
			_particleFactory = particleFactory;
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			MapProvider.Map.CurrentValue.MapUpdated
				.Where(_ => IsSuspended())
				.Subscribe(_ => ExplodeWithFx())
				.AddTo(this);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		[ServerCallback]
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

			ExplodeWithFx();
		}

		private bool IsSuspended()
		{
			var voxelPosition = Vector3Ushort.FloorToUshort(transform.position - Vector3.Scale(transform.up, Map.WorldOffset));
			VoxelData voxelData = MapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(voxelPosition);
			return !voxelData.IsSolid();
		}

		private void ExplodeWithFx()
		{
			Explode(_configure.ExplosionData);

			_particleFactory.CreateRchParticle(transform.position, _configure.ParticleSpeed, _configure.ParticleCount,
				_configure.ExplosionData.radius);
			_audioSender.SendAudio(AudioType.TNTExplosion, transform.position);

			Destroy(gameObject);
		}
	}
}
