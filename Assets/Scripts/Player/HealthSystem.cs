using Data;
using Mirror;
using Networking.Synchronization;
using UnityEngine;

namespace Player
{
    public class HealthSystem :MonoBehaviour
    {
        public int Health { get; set; }
        [SerializeField] private HealthSynchronization healthSynchronization;
        private int _maxHealth;

        public void Construct(PlayerCharacteristic characteristic)
        {
            _maxHealth = characteristic.health;
        }
        private void Start()
        {
            _maxHealth = Health;
        }
        
    }
}