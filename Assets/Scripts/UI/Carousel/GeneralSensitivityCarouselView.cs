namespace UI.Carousel
{
    public class GeneralSensitivityCarouselView : CarouselView<int>
    {
        public GeneralSensitivityCarouselView(CarouselControl control) : base(control)
        {
        }

        public override void OnModelValueChanged(int sensitivity)
        {
            Control.DisplayedValue.SetText(sensitivity.ToString());
        }
    }
}