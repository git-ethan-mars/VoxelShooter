using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Data;
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

        private CancellationToken _token;

        public void Construct()
        {
            canvasGroup.alpha = 0.0f;
            _token = this.GetCancellationTokenOnDestroy();
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
                EnableRespawnTimer().Forget();
            }
        }

        private async UniTaskVoid EnableRespawnTimer()
        {
            if (await UniTask.WaitForSeconds(1, cancellationToken: _token).SuppressCancellationThrow())
            {
                return;
            }
            
            respawnTimeText.gameObject.SetActive(false);
        }
    }
}