using System;
using Mirror;
using Networking.Core;
using R3;
namespace GamePlay
{
	public class HealthSystem : NetworkBehaviour
	{
		public ReactiveProperty<int> Health => _health;
		
		private readonly SyncReactiveProperty<int> _health = new SyncReactiveProperty<int>();
		private int _maxHealth = 100;

		[Server]
		public void Initialize(int maxHealth)
		{
			_maxHealth = maxHealth;
			Health.Value = maxHealth;
		}

		[Server]
		public void Increase(int healValue)
		{
			if (healValue <= 0)
			{
				throw new ArgumentException("Heal value should be positive", nameof(healValue));
			}

			Health.Value = Math.Min(Health.Value + healValue, _maxHealth);
		}

		[Server]
		public void Decrease(int damageValue)
		{
			if (damageValue <= 0)
			{
				throw new ArgumentException("Damage value should be positive", nameof(damageValue));
			}

			Health.Value = Math.Max(Health.Value - damageValue, 0);
		}
	}
}