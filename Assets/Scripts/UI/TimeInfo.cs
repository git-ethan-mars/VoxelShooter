using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
namespace UI
{
	public class TimeInfo : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI serverTimeText;
		[SerializeField] private TextMeshProUGUI respawnTimeText;
		[SerializeField] private CanvasGroup canvasGroup;

		public CanvasGroup CanvasGroup => canvasGroup;

		public void ChangeGameTime(TimeSpan timeLeft)
		{
			serverTimeText.SetText($"{timeLeft.Minutes}:{timeLeft.Seconds:00}");
		}

		public async void ChangeRespawnTime(TimeSpan timeLeft)
		{
			try
			{
				if (!respawnTimeText.gameObject.activeSelf)
				{
					respawnTimeText.gameObject.SetActive(true);
				}

				while (timeLeft.TotalSeconds > 0)
				{
					timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
					respawnTimeText.SetText($"You will respawn in {timeLeft.TotalSeconds}");
					if (await UniTask.WaitForSeconds(1, cancellationToken: destroyCancellationToken).SuppressCancellationThrow())
					{
						return;
					}
				}

				respawnTimeText.gameObject.SetActive(false);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}