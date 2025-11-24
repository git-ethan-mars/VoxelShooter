using System;
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

		public void ChangeGameTime(TimeSpan gameTime)
		{
			serverTimeText.SetText($"{gameTime.Minutes}:{gameTime.Seconds:00}");
		}

		public void ChangeRespawnTime(TimeSpan respawnTime)
		{
			if (respawnTime == TimeSpan.Zero)
			{
				respawnTimeText.gameObject.SetActive(false);
				return;
			}

			if (respawnTime > TimeSpan.Zero && !respawnTimeText.gameObject.activeSelf)
			{
				respawnTimeText.gameObject.SetActive(true);
			}

			respawnTimeText.SetText($"You will respawn in {(int)respawnTime.TotalSeconds}");
		}
	}
}