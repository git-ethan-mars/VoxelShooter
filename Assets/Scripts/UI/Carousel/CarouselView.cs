using R3;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Carousel
{
	public abstract class CarouselView<T> : MonoBehaviour
	{
		public Observable<Unit> IncreaseButtonPressed;
		public Observable<Unit> DecreaseButtonPressed;
		[SerializeField] protected Button increaseButton;
		[SerializeField] protected Button decreaseButton;

		private void Awake()
		{
			IncreaseButtonPressed = increaseButton.onClick.AsObservable();
			DecreaseButtonPressed = decreaseButton.onClick.AsObservable();
		}

		public abstract void OnModelValueChanged(T value);
	}
}
