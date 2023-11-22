using PlayerLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HealthCounter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI healthText;

        private Player _player;

        public void Construct(Player player)
        {
            _player = player;
            healthText.SetText(_player.Health.ToString());
            _player.Health.ValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth)
        {
            healthText.SetText(currentHealth.ToString());
        }

        private void OnDestroy()
        {
            _player.Health.ValueChanged -= OnHealthChanged;
        }
    }
}