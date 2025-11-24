using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.MapFeatures;
using Mirror;
using Networking.Audio;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;
using AudioType = Data.AudioType;
namespace GamePlay
{
	public class Drill : Entity
	{
		[SerializeField] private ParticleSystem particles;
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private BoxCollider boxCollider;

		private MapProvider _mapProvider;
		private EntityContainerService _entityContainer;
		private DrillLauncherConfigure _configure;
		private NetworkAudioPlayer _audioPlayer;

		[Inject]
		private void Construct(MapProvider mapProvider, IStaticDataService staticData, EntityContainerService entityContainer,
			NetworkAudioPlayer audioPlayer)
		{
			_mapProvider = mapProvider;
			_entityContainer = entityContainer;
			_configure = staticData.GetItemConfigure<DrillLauncherConfigure>(ItemType.DrillLauncher);
			_audioPlayer = audioPlayer;
		}

		private void Start()
		{
			_entityContainer.Add(this);
		}

		private void OnDestroy()
		{
			_entityContainer.Remove(this);
		}

		[ServerCallback]
		private void FixedUpdate()
		{
			rigidBody.AddForce(Vector3.down);
			float previousZAngle = rigidBody.rotation.eulerAngles.z;
			rigidBody.rotation = Quaternion.LookRotation(rigidBody.linearVelocity)
			                     * Quaternion.Euler(new Vector3(0, -180, _configure.RotationSpeed + previousZAngle));

			if (rigidBody.position.y < 0)
			{
				Destroy(gameObject);
			}
		}
		
		[ServerCallback]
		private void OnTriggerEnter(Collider other)
		{
			if (_mapProvider.Map.TryGetFeature(out MapDestruction mapDestruction))
			{
				mapDestruction.Visit(_configure.ExplosionData, transform.position);
			}

			foreach (IDamageVisitor visitor in _entityContainer.GetEntitiesByType<IDamageVisitor>())
			{
				visitor.Visit(_configure.ExplosionData, transform.position);
			}
			
			_audioPlayer.SendAudio(AudioType.DrillHit, transform.position);
		}

		public void Launch()
		{
			rigidBody.linearVelocity = transform.forward * _configure.Speed;
			DestroyAsync(destroyCancellationToken).Forget();
		}

		private async UniTaskVoid DestroyAsync(CancellationToken token)
		{
			await UniTask.WaitForSeconds(_configure.LifeTime, cancellationToken: token);
			Destroy(gameObject);
		}

		public override Bounds Bounds => boxCollider.bounds;
	}
}