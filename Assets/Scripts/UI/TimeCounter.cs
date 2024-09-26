using Common.StaticData;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace UI
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

        public void Construct()
        {
            canvasGroup.alpha = 0.0f;
        }

        public void ChangeGameTime(ServerTime timeLeft)
        {
            serverTimeText.SetText($"{timeLeft.Minutes}:{timeLeft.Seconds:00}");
        }

        public void ChangeRespawnTime(ServerTime timeLeft)
        {
            if (!respawnTimeText.gameObject.activeSelf)
            {
                respawnTimeText.gameObject.SetActive(true);
            }

            respawnTimeText.SetText($"You will respawn in {timeLeft.TotalSecond}");
            if (timeLeft.TotalSecond == 0)
            {
                EnableRespawnTimer();
            }
        }

        private async UniTaskVoid EnableRespawnTimer()
        {
            await UniTask.WaitForSeconds(1);
            respawnTimeText.gameObject.SetActive(false);
        }
    }
}