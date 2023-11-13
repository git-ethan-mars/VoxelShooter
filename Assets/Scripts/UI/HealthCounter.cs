using Networking;
using PlayerLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HealthCounter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI healthText;

        private IClient _client;

        public void Construct(IClient client, Player player)
        {
            _client = client;
            healthText.SetText(player.Health.ToString());
            _client.HealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth)
        {
            healthText.SetText(currentHealth.ToString());
        }

        private void OnDestroy()
        {
            _client.HealthChanged -= OnHealthChanged;
        }
    }
}