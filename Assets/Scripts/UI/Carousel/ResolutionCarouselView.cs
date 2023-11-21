using UI.SettingsMenu.States;
using UnityEngine;

namespace UI.Carousel
{
    public class ResolutionCarouselView : CarouselView<Resolution>
    {
        public ResolutionCarouselView(CarouselControl control) : base(control)
        {
        }

        public override void OnModelValueChanged(Resolution resolution)
        {
            Control.DisplayedValue.SetText($"{resolution.width}X{resolution.height} {resolution.refreshRate}Hz");
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        }
    }
}