using Data;
using Infrastructure.Services.Input;
using Networking;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TimeCounter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI serverTimeText;

        [SerializeField]
        private TextMeshProUGUI respawnTimeText;

        private IInputService _inputService;
        private CanvasGroup _canvasGroup;
        private IClient _client;

        public void Construct(IClient client, IInputService inputService)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _inputService = inputService;
            _client = client;
            _client.GameFinished += HideTimer;
            _client.GameTimeChanged += ChangeGameTime;
            _client.RespawnTimeChanged += ChangeRespawnTime;
        }

        public void Update()
        {
            _canvasGroup.alpha = _inputService.IsScoreboardButtonHold() ? 0 : 1;
        }

        private void ChangeGameTime(ServerTime timeLeft)
        {
            serverTimeText.SetText($"{timeLeft.Minutes}:{timeLeft.Seconds:00}");
        }

        private void HideTimer()
        {
            _client.GameFinished -= HideTimer;
            gameObject.SetActive(false);
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

        private void OnDestroy()
        {
            _client.GameTimeChanged -= ChangeGameTime;
            _client.RespawnTimeChanged -= ChangeRespawnTime;
        }
    }
}