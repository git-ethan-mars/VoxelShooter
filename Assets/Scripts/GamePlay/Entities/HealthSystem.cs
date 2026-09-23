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
			_health.Value = maxHealth;
		}

		[Server]
		public void Increase(int healValue)
		{
			if (healValue <= 0)
			{
				throw new ArgumentException("Heal value should be positive", nameof(healValue));
			}

			_health.Value = Math.Min(_health.Value + healValue, _maxHealth);
		}

		[Server]
		public void Decrease(int damageValue)
		{
			if (damageValue <= 0)
			{
				throw new ArgumentException("Damage value should be positive", nameof(damageValue));
			}

			_health.Value = Math.Max(_health.Value - damageValue, 0);
		}
	}
}