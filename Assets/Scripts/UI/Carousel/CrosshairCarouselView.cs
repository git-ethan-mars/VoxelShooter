using Data;
using UnityEngine;
using UnityEngine.UI;
namespace UI.Carousel
{
	public class CrosshairCarouselView : CarouselView<CrosshairSprite>
	{
		[SerializeField] private Image image;

		public override void OnModelValueChanged(CrosshairSprite crosshairSprite)
		{
			image.sprite = crosshairSprite.Sprite;
		}
	}
}