using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class PaletteElementView : MonoBehaviour
	{
		private const float SwitchColorTime = 0.25f;

		[SerializeField] private Image colorIcon;
		[SerializeField] private Image boarder;
		[SerializeField] private Sprite blackBoarder;
		[SerializeField] private Sprite blueBoarder;

		public void Construct(Color color)
		{
			colorIcon.color = color;
		}

		public async UniTask RunAnimationAsync(CancellationToken token)
		{
			boarder.gameObject.SetActive(true);

			try
			{
				while (!token.IsCancellationRequested)
				{
					if (boarder == null)
					{
						return;
					}

					boarder.sprite = blackBoarder;
					await UniTask.WaitForSeconds(SwitchColorTime, cancellationToken: token);

					if (boarder == null)
					{
						return;
					}

					boarder.sprite = blueBoarder;
					await UniTask.WaitForSeconds(SwitchColorTime, cancellationToken: token);
				}
			}

			catch (OperationCanceledException)
			{
				boarder.gameObject.SetActive(false);
			}
		}
	}
}