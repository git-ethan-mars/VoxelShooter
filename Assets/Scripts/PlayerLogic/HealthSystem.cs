using System;
using Data;
using UnityEngine;

namespace PlayerLogic
{
    public class HealthSystem :MonoBehaviour
    {
        public event Action<int, int> OnHealthChanged; 
        public int Health { get; set; }
        private int _maxHealth;
        

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

            Debug.Log($"{Health}");
            OnHealthChanged?.Invoke(Health, _maxHealth);
        }
        
    }
}