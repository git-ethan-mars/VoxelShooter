using R3;
using UnityEngine;
using UnityEngine.UI;
namespace UI.Carousel
{
	public abstract class CarouselView<T> : MonoBehaviour
	{
		[SerializeField] protected Button increaseButton;
		[SerializeField] protected Button decreaseButton;

		private void Awake()
		{
			IncreaseButtonPressed = increaseButton.onClick.AsObservable();
			DecreaseButtonPressed = decreaseButton.onClick.AsObservable();
		}

		public Observable<Unit> IncreaseButtonPressed;
		public Observable<Unit> DecreaseButtonPressed;

		public abstract void OnModelValueChanged(T value);
	}
}