using System;
using Data;
using Mirror;

namespace PlayerLogic
{
    public class HealthSystem : NetworkBehaviour
    {
        public event Action<int, int> OnHealthChanged;
        private int Health { get; set; }
        [SyncVar] private int _maxHealth;
        

        public void Construct(PlayerCharacteristic characteristic)
        {
            _maxHealth = characteristic.maxHealth;
        }
        private void Start()
        {
            _maxHealth = Health;
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

            OnHealthChanged?.Invoke(Health, _maxHealth);
        }
        
    }
}