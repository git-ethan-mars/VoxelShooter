using TMPro;
using UnityEngine;
namespace UI.Carousel
{
	public class ResolutionCarouselView : CarouselView<Resolution>
	{
		[SerializeField] private TextMeshProUGUI displayedText;

		public override void OnModelValueChanged(Resolution crosshairSprite)
		{
			displayedText.SetText($"{crosshairSprite.width}X{crosshairSprite.height} {crosshairSprite.refreshRateRatio}Hz");
		}
	}
}