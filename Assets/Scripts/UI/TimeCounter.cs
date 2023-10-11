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
        private CustomNetworkManager _networkManager;

        public void Construct(IInputService inputService, CustomNetworkManager networkManager)
        {
            _inputService = inputService;
            _networkManager = networkManager;
            _canvasGroup = GetComponent<CanvasGroup>();
            _networkManager.GameFinished += HideTimer;
            _networkManager.Client.GameTimeChanged += ChangeGameTime;
            _networkManager.Client.RespawnTimeChanged += ChangeRespawnTime;
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
            _networkManager.GameFinished -= HideTimer;
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
                StartCoroutine(Utils.DoActionAfterDelay(1, () => respawnTimeText.gameObject.SetActive(false)));
            }
        }

        private void OnDestroy()
        {
            _networkManager.Client.GameTimeChanged -= ChangeGameTime;
            _networkManager.Client.RespawnTimeChanged -= ChangeRespawnTime;
        }
    }
}