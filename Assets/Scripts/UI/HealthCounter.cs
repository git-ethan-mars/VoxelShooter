using PlayerLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HealthCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI healthText;
        
        public void Construct(GameObject player)
        {
            player.GetComponent<Player>().OnHealthChanged += OnHealthChanged; 
            OnHealthChanged(player.GetComponent<Player>().health);
        }

        private void OnHealthChanged(int currentHealth)
        {
            healthText.SetText(currentHealth.ToString());
        }
    }
}
