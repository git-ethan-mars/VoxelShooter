using System;
using R3;
namespace UI.Carousel
{
	public class CarouselModel<T>
	{
		private readonly T[] _options;
		private int _currentIndex;

		public CarouselModel(T initialValue, params T[] options)
		{
			CurrentItem = new ReactiveProperty<T>(initialValue);
			_currentIndex = Array.FindIndex(options, item => CurrentItem.Value.Equals(item));
			_options = options;
		}

		public ReactiveProperty<T> CurrentItem { get; }

		public void MoveForward()
		{
			_currentIndex = (_currentIndex + 1 + _options.Length) % _options.Length;
			CurrentItem.Value = _options[_currentIndex];
		}

		public void MoveBack()
		{
			_currentIndex = (_currentIndex - 1 + _options.Length) % _options.Length;
			CurrentItem.Value = _options[_currentIndex];
		}
	}
}