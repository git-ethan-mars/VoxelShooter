using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;
using AudioType = Data.AudioType;

namespace GamePlay
{
	public class Drill : Explosive
	{
		[SerializeField] private ParticleSystem particles;
		[SerializeField] private Rigidbody rigidBody;
		[SerializeField] private BoxCollider boxCollider;

		private DrillLauncherConfigure _configure;
		private NetworkAudioSender _audioSender;

		public override Bounds Bounds => boxCollider.bounds;
		public override ExplosiveType Type => ExplosiveType.Drill;

		[Inject]
		private void Construct(MapProvider mapProvider, IStaticDataService staticData, EntityContainer entityContainer,
			NetworkAudioSender audioSender)
		{
			MapProvider = mapProvider;
			EntityContainer = entityContainer;
			_configure = staticData.GetItemConfigure<DrillLauncherConfigure>(ItemType.DrillLauncher);
			_audioSender = audioSender;
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
			Explode(_configure.ExplosionData);

			_audioSender.SendAudio(AudioType.DrillHit, transform.position);
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
	}
}
