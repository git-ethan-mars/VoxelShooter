using Data;
using Networking;
using TMPro;
using UnityEngine;

namespace UI.Windows
{
    public class TimeCounter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI serverTimeText;

        [SerializeField]
        private TextMeshProUGUI respawnTimeText;

        public CanvasGroup CanvasGroup => canvasGroup;

        [SerializeField]
        private CanvasGroup canvasGroup;

        private IClient _client;

        public void Construct(IClient client)
        {
            _client = client;
            _client.GameFinished += HideTimer;
            _client.GameTimeChanged += ChangeGameTime;
            _client.RespawnTimeChanged += ChangeRespawnTime;
            canvasGroup.alpha = 0.0f;
        }

        private void ChangeGameTime(ServerTime timeLeft)
        {
            serverTimeText.SetText($"{timeLeft.Minutes}:{timeLeft.Seconds:00}");
        }

        private void ChangeRespawnTime(ServerTime timeLeft)
        {
            if (!respawnTimeText.gameObject.activeSelf)
            {
                respawnTimeText.gameObject.SetActive(true);
            }

            respawnTimeText.SetText($"You will respawn in {timeLeft.TotalSecond}");
            if (timeLeft.TotalSecond == 0)
            {
                StartCoroutine(Utils.DoActionAfterDelay(() => respawnTimeText.gameObject.SetActive(false), 1));
            }
        }

        private void HideTimer()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _client.GameFinished -= HideTimer;
            _client.GameTimeChanged -= ChangeGameTime;
            _client.RespawnTimeChanged -= ChangeRespawnTime;
        }
    }
}