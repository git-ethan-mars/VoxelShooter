using TMPro;
using UnityEngine;
namespace UI.Carousel
{
	public class ScreenModeCarouselView : CarouselView<FullScreenMode>
	{
		private const string WindowedMode = "Windowed";
		private const string FullScreen = "Full screen";

		[SerializeField] private TextMeshProUGUI displayedText;

		public override void OnModelValueChanged(FullScreenMode crosshairSprite)
		{
			string screenModeText = null;
			if (crosshairSprite == FullScreenMode.Windowed)
			{
				screenModeText = WindowedMode;
			}

			if (crosshairSprite == FullScreenMode.FullScreenWindow)
			{
				screenModeText = FullScreen;
			}

			displayedText.SetText(screenModeText);
		}
	}
}