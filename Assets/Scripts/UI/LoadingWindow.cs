using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class LoadingWindow : ListView<Image>
	{
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private int bulletCount;
		[SerializeField, Range(0, 1)] private float hideAlpha;
		[SerializeField, Range(0, 1)] private float showAlpha;

		private float _previousProgress;

		private void Awake()
		{
			for (int i = 0; i < bulletCount; i++)
			{
				SpawnElement();
			}
		}

		public void Show()
		{
			canvasGroup.alpha = 1;
			
			for (int i = 0; i < bulletCount; i++)
			{
				Color currentColor = Items[i].color;
				Items[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, hideAlpha);
			}
		}

		public void Hide()
		{
			canvasGroup.alpha = 0;
		}

		public void UpdateLoadingBar(float progress)
		{
			if (Mathf.Approximately(_previousProgress, progress))
			{
				return;
			}

			int bulletProgressIndex = (int)Math.Ceiling(progress * bulletCount);

			for (int i = 0; i < bulletProgressIndex; i++)
			{
				Color currentColor = Items[i].color;
				Items[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, showAlpha);
			}

			for (int i = bulletProgressIndex; i < bulletCount; i++)
			{
				Color currentColor = Items[i].color;
				Items[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, hideAlpha);
			}

			_previousProgress = progress;
		}
	}
}