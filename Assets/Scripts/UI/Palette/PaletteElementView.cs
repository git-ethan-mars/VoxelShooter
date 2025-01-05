using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class PaletteElementView : MonoBehaviour
	{
		private const float SwitchColorTime = 0.25f;

		[SerializeField] 
		private Image colorIcon;

		[SerializeField] 
		private Image boarder;

		[SerializeField] 
		private Sprite blackBoarder;

		[SerializeField] 
		private Sprite blueBoarder;

		private bool _stopAnimation;

		public void Construct(Color color, Vector2 size)
		{
			colorIcon.color = color;
			boarder.rectTransform.sizeDelta = size;
		}

		public async UniTaskVoid StartAnimationAsync()
		{
			boarder.gameObject.SetActive(true);
			while (!_stopAnimation)
			{
				boarder.sprite = blackBoarder;
				await UniTask.WaitForSeconds(SwitchColorTime);
				boarder.sprite = blueBoarder;
				await UniTask.WaitForSeconds(SwitchColorTime);
			}
			
			boarder.gameObject.SetActive(false);
			_stopAnimation = false;
		}

		public void StopAnimation()
		{
			_stopAnimation = true;
		}
	}
}