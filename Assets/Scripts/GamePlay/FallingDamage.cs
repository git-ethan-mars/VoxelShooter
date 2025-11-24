using Mirror;
using UnityEngine;
namespace GamePlay
{
	public class FallingDamage : NetworkBehaviour
	{
		private const float SpeedThreshold = 1e-3f;
		
		[SerializeField] private int minSpeedToDamage = 25;
		[SerializeField] private int damagePerMetersPerSecond = 3;
		[SerializeField] private Rigidbody rigidBody;
		
		private IDamageaeble _damageaeble;

		private float _previousSpeed;

		private void Awake()
		{
			_damageaeble = GetComponent<IDamageaeble>();
		}

		[ServerCallback]
		private void Update()
		{
			if (_previousSpeed < -minSpeedToDamage && Mathf.Abs(rigidBody.linearVelocity.y) < SpeedThreshold)
			{
				int damage = Mathf.CeilToInt(-(_previousSpeed + minSpeedToDamage) * damagePerMetersPerSecond);
				_damageaeble.Damage(damage);
			}

			_previousSpeed = rigidBody.linearVelocity.y;
		}
	}
}