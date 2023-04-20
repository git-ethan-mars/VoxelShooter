using PlayerLogic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        public void Construct(GameObject player)
        {
            player.GetComponent<HealthSystem>().OnHealthChanged += OnHealthChanged; 
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}