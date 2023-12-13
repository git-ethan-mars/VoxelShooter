namespace UI.Carousel
{
    public class AimSensitivityCarouselView : CarouselView<int>
    {
        public AimSensitivityCarouselView(CarouselControl control) : base(control)
        {
        }

        public override void OnModelValueChanged(int sensitivity)
        {
            Control.DisplayedValue.SetText(sensitivity.ToString());
        }
    }
}