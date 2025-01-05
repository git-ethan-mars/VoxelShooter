using System;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Entities
{
	public class Drill : Entity
	{
		public event Action Collided;


		[SerializeField]
		private ParticleSystem particles;

		[SerializeField]
		private Rigidbody rigidBody;

		public DrillLauncherData Data { get; private set; }


		public void Construct(DrillLauncherData drillLauncherData)
		{
			Data = drillLauncherData;
		}

		private void FixedUpdate()
		{
			rigidBody.AddForce(Vector3.down);
			var previousZAngle = rigidBody.rotation.eulerAngles.z;
			rigidBody.rotation = Quaternion.LookRotation(rigidBody.linearVelocity)
			                     * Quaternion.Euler(new Vector3(0, -180, Data.RotationSpeed + previousZAngle));
		}
	}
}