using GamePlay;
using GamePlay.Services;
using Mirror;
using UnityEngine;

namespace Networking.Host.Services
{
	public class FallDamageComponent : NetworkBehaviour
	{
		private const float SpeedThreshold = 1e-3f;
		
		[SerializeField] private Character character;

		private IStaticDataService _staticData;
		private int _damagePerMetersPerSecond;
		private int _minSpeedToDamage;
		private float _previousSpeed;

		public void Construct(IStaticDataService staticData)
		{
			_staticData = staticData;
			var fallDamageConfiguration = _staticData.GetFallDamageConfiguration();
			_minSpeedToDamage = fallDamageConfiguration.minSpeedToDamage;
			_damagePerMetersPerSecond = fallDamageConfiguration.damagePerMetersPerSecond;
		}


		[ServerCallback]
		private void Update()
		{
			if (_previousSpeed < -_minSpeedToDamage
			    && Mathf.Abs(character.RigidBody.linearVelocity.y) < SpeedThreshold)
			{
				var damage = (int)(-(_previousSpeed + _minSpeedToDamage) *
				                   _damagePerMetersPerSecond);
				character.Damage(damage);
			}

			_previousSpeed = character.RigidBody.linearVelocity.y;
		}
	}
}