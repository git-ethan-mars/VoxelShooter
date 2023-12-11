using Data;
using Networking;
using TMPro;
using UnityEngine;

namespace UI.InGameUI
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

        private CustomNetworkManager _networkManager;

        public void Construct(CustomNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.Client.GameTimeChanged += ChangeGameTime;
            _networkManager.Client.RespawnTimeChanged += ChangeRespawnTime;
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

        private void OnDestroy()
        {
            _networkManager.Client.GameTimeChanged -= ChangeGameTime;
            _networkManager.Client.RespawnTimeChanged -= ChangeRespawnTime;
        }
    }
}