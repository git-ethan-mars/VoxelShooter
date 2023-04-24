using System;
using Data;
using Mirror;

namespace PlayerLogic
{
    public class HealthSystem : NetworkBehaviour
    {
        public event Action<int> OnHealthChanged;
        public int Health { get; private set;  }
        [SyncVar] private int _maxHealth;
        

        public void Construct(PlayerCharacteristic characteristic)
        {
            _maxHealth = characteristic.maxHealth;
        }
        private void Start()
        {
            Health = _maxHealth;
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            _maxHealth = maxHealth;
            if (currentHealth >= _maxHealth)
                Health = _maxHealth;
            else
            {
                Health = currentHealth;
            }

            OnHealthChanged?.Invoke(Health);
        }
        
    }
}