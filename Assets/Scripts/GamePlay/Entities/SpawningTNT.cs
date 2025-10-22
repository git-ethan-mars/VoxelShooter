using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Audio;
using GamePlay.MapFeatures;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using TMPro;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	public class SpawningTNT : Entity
	{
		[SerializeField] private Bounds localBounds;
		
		[SerializeField] private Canvas canvas;
		[SerializeField] private TextMeshProUGUI timerText;

		[SerializeField] private AudioData explosionAudio;
		[SerializeField] private AudioData countdownAudio;

		private MapProvider _mapProvider;
		private EntityContainerService _entityContainer;
		private AudioPlayer _audioPlayer;
		private IParticleFactory _particleFactory;
		private TNTConfigure _configure;

		private IDisposable _disposable;

		[Inject]
		private void Construct(IStaticDataService staticData, MapProvider mapProvider, EntityContainerService entityContainer,
			AudioPlayer audioPlayer, IParticleFactory particleFactory)
		{
			_configure = staticData.GetItemConfigure<TNTConfigure>(ItemType.TNT);
			_mapProvider = mapProvider;
			_entityContainer = entityContainer;
			_audioPlayer = audioPlayer;
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

			_disposable = _mapProvider.Map.MapUpdated
				.Where(_ => IsSuspended())
				.Subscribe(_ => Explode());
		}

		private bool IsSuspended()
		{
			Vector3Int voxelPosition = Vector3Int.FloorToInt(transform.position - Vector3.Scale(transform.up, Map.WorldOffset));
			VoxelData voxelData = _mapProvider.Map.GetVoxelByGlobalPosition(voxelPosition);
			return !voxelData.IsSolid();
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
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		public override Bounds Bounds => new Bounds(transform.position + localBounds.center, localBounds.size);
	}
}