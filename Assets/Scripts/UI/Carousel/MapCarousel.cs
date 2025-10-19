using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace UI.Carousel
{
	public class MapCarouselView : CarouselView<MapView>
	{
		[SerializeField] private TMP_Text mapName;
		[SerializeField] private Image mask;
		[SerializeField] private Image previousMapImage;
		[SerializeField] private Image mapImage;
		[SerializeField] private float animationDuration;

		public override void OnModelValueChanged(MapView mapView)
		{
			previousMapImage.sprite = mapImage.sprite;
			mapName.SetText(mapView.MapName);
			mapImage.sprite = mapView.Icon;
		}

		public async UniTask PlayMapImageAnimationAsync(bool inverse, CancellationToken token)
		{
			increaseButton.interactable = false;
			decreaseButton.interactable = false;

			mapImage.transform.localPosition = new Vector3((inverse ? -1 : 1) * mask.rectTransform.rect.width, 0, 0);
			previousMapImage.transform.localPosition = Vector3.zero;

			UniTask slideAnimation = DOTween.Sequence()
				.Append(mapImage.transform
					.DOLocalMoveX(0, animationDuration)
					.SetEase(Ease.InOutSine))
				.Insert(0, previousMapImage.transform.DOLocalMoveX((inverse ? 1 : -1) * mask.rectTransform.rect.width, animationDuration)
					.SetEase(Ease.InOutSine))
				.ToUniTask(cancellationToken: token, tweenCancelBehaviour: TweenCancelBehaviour.Complete);

			await slideAnimation;

			increaseButton.interactable = true;
			decreaseButton.interactable = true;

			EventSystem.current.SetSelectedGameObject(inverse ? decreaseButton.gameObject : increaseButton.gameObject);
		}
	}
}