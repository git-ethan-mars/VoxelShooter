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
        [SerializeField] private TextMeshProUGUI serverTimeText;
        [SerializeField] private TextMeshProUGUI respawnTimeText;
        private IInputService _inputService;
        private CanvasGroup _canvasGroup;

        public void Construct(IInputService inputService, CustomNetworkManager networkManager)
        {
            _inputService = inputService;
            _canvasGroup = GetComponent<CanvasGroup>();
            networkManager.ServerTimeChanged += ChangeServerTime;
            networkManager.RespawnTimeChanged += ChangeRespawnTime;
        }

        public void Update()
        {
            _canvasGroup.alpha = _inputService.IsScoreboardButtonHold() ? 0 : 1;
        }

        private void ChangeServerTime(ServerTime timeLeft)
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
                StartCoroutine(Utils.DoActionAfterDelay(1, () => respawnTimeText.gameObject.SetActive(false)));
            }
        }
    }
}