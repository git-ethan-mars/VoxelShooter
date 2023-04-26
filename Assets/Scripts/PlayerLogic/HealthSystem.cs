using System;
using Data;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerLogic
{
    public class HealthSystem : NetworkBehaviour
    {
        public event Action<int> OnHealthChanged;
        [SyncVar] public int health;
        [SyncVar] private int _maxHealth;
        

        public void Construct(PlayerCharacteristic characteristic)
        {
            _maxHealth = characteristic.maxHealth;
            health = _maxHealth;
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            _maxHealth = maxHealth;
            health = currentHealth >= _maxHealth ? _maxHealth : currentHealth;
            OnHealthChanged?.Invoke(health);
        }
        
    }
}