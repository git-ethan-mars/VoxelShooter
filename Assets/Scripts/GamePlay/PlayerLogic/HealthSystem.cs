using System;

namespace PlayerLogic
{
	public class HealthSystem
	{
		public event Action<int> HealthChanged;

		private readonly int _maxHealth;
		public int Health { get; private set; }

		public HealthSystem(int maxHealth)
		{
			_maxHealth = maxHealth;
			Health = maxHealth;
		}

		public void Increase(int healValue)
		{
			if (healValue <= 0)
			{
				throw new ArgumentException("Heal value should be positive", nameof(healValue));
			}

			Health = Math.Min(Health + healValue, _maxHealth);
			HealthChanged?.Invoke(Health);
		}

		public void Decrease(int damageValue)
		{
			if (damageValue <= 0)
			{
				throw new ArgumentException("Damage value should be positive", nameof(damageValue));
			}

			Health = Math.Max(Health - damageValue, 0);
			HealthChanged?.Invoke(Health);
		}
	}
}