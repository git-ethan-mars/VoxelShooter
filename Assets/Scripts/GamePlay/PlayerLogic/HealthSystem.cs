using System;

namespace GamePlay
{
    public class HealthSystem
    {
        public event Action<int> HealthChanged;

        private readonly int _maxHealth;
        private int _health;

        public HealthSystem(int maxHealth)
        {
            _maxHealth = maxHealth;
            _health = maxHealth;
        }

        public void Increase(int healValue)
        {
            if (healValue <= 0)
            {
                throw new ArgumentException("Heal value should be positive", nameof(healValue));
            }

            _health = Math.Min(_health + healValue, _maxHealth);
            HealthChanged?.Invoke(_health);
        }

        public void Decrease(int damageValue)
        {
            if (damageValue <= 0)
            {
                throw new ArgumentException("Damage value should be positive", nameof(damageValue));
            }

            _health = Math.Max(_health - damageValue, 0);
            HealthChanged?.Invoke(_health);
        }
    }
}