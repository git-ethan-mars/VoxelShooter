using UnityEngine;

namespace GamePlay
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private int health;

        public int Health
        {
            get => health;
            set => health = value;
        }

        public int MaxHealth { get; set; }

        private void Awake()
        {
            MaxHealth = health;
        }
    }
}