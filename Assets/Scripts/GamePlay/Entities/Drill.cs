using System;
using Common.Audio;
using Common.StaticData;
using GamePlay.Entities;
using Inventory;
using UnityEngine;

namespace Entities
{
	public class Drill : Entity, IInventoryItem
	{
		public event Action Collided;

		[SerializeField]
		private ParticleSystem particles;

		[SerializeField]
		private Rigidbody rigidBody;

		private IAudioPlayer _audioPlayer;
		public DrillLauncherData Data { get; private set; }

		public void Construct(IAudioPlayer audioPlayer, DrillLauncherData drillLauncherData)
		{
			_audioPlayer = audioPlayer;
			Data = drillLauncherData;
		}

		private void FixedUpdate()
		{
			rigidBody.AddForce(Vector3.down);
			var previousZAngle = rigidBody.rotation.eulerAngles.z;
			rigidBody.rotation = Quaternion.LookRotation(rigidBody.velocity)
			                     * Quaternion.Euler(new Vector3(0, -180, Data.RotationSpeed + previousZAngle));
		}

		public void Enable()
		{
			throw new NotImplementedException();
		}

		public void Disable()
		{
			throw new NotImplementedException();
		}
	}
}