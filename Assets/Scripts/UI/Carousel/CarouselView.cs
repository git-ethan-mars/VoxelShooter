using UnityEngine.Events;

namespace UI.Carousel
{
    public abstract class CarouselView<T>
    {
        public event UnityAction IncreaseButtonPressed
        {
            add => Control.IncreaseButton.onClick.AddListener(value);
            remove => Control.IncreaseButton.onClick.RemoveListener(value);
        }

        public event UnityAction DecreaseButtonPressed
        {
            add => Control.DecreaseButton.onClick.AddListener(value);
            remove => Control.DecreaseButton.onClick.RemoveListener(value);
        }

        public abstract void OnModelValueChanged(T value);

        protected readonly CarouselControl Control;

        protected CarouselView(CarouselControl control)
        {
            Control = control;
        }
    }
}