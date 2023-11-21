using UnityEngine;

namespace UI.Carousel
{
    public class ScreenModeCarouselView : CarouselView<FullScreenMode>
    {
        private const string WindowedMode = "Windowed";
        private const string FullScreen = "Full screen";

        public ScreenModeCarouselView(CarouselControl control) : base(control)
        {
        }

        public override void OnModelValueChanged(FullScreenMode screenMode)
        {
            Screen.fullScreenMode = screenMode;
            string screenModeText = default;
            if (screenMode == FullScreenMode.Windowed)
            {
                screenModeText = WindowedMode;
            }

            if (screenMode == FullScreenMode.FullScreenWindow)
            {
                screenModeText = FullScreen;
            }

            Control.DisplayedValue.SetText(screenModeText);
        }
    }
}