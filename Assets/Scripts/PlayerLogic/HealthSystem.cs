using System;
using Data;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class HealthSystem : NetworkBehaviour
    {
        public event Action<int> OnHealthChanged;
        [HideInInspector] [SyncVar(hook = nameof(UpdateHealth))] public int health;
        [SyncVar] private int _maxHealth;
        

        public void Construct(PlayerCharacteristic characteristic)
        {
            _maxHealth = characteristic.maxHealth;
            health = _maxHealth;
        }

        private void UpdateHealth(int oldHealth, int newHealth)
        {
            OnHealthChanged?.Invoke(newHealth);
        }

    }
}