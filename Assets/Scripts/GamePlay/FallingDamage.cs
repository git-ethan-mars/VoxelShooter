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

		[field: SerializeField] public Entity Entity { get; private set; }

		private float _previousSpeed;
		private IDamageVisitor _damageVisitor;

		private void Awake()
		{
			_damageVisitor = Entity.GetComponent<IDamageVisitor>();

			if (_damageVisitor == null)
			{
				Debug.LogWarning("No IDamageVisitor script attached to " + gameObject.name);
			}
		}

		[ServerCallback]
		private void Update()
		{
			if (_previousSpeed < -minSpeedToDamage && Mathf.Abs(rigidBody.linearVelocity.y) < SpeedThreshold)
			{
				int damage = Mathf.CeilToInt(-(_previousSpeed + minSpeedToDamage) * damagePerMetersPerSecond);
				_damageVisitor?.Visit(this, damage);
			}

			_previousSpeed = rigidBody.linearVelocity.y;
		}
	}
}