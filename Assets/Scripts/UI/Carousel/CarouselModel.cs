using System;
using System.Collections.Generic;

namespace UI.Carousel
{
    public class CarouselModel<T>
    {
        public event Action<T> ValueChanged;
        private readonly List<T> _items;
        private int _currentIndex;

        public CarouselModel(List<T> items)
        {
            _items = items;
        }

        public T CurrentItem
        {
            get => _currentItem;
            set
            {
                if (!_currentItem.Equals(value))
                {
                    _currentItem = value;
                    ValueChanged?.Invoke(_currentItem);
                }
            }
        }

        private T _currentItem;

        public void MoveForward()
        {
            _currentIndex = (_items.FindIndex(item => item.Equals(_currentItem)) + 1 + _items.Count) % _items.Count;
            CurrentItem = _items[_currentIndex];
        }

        public void MoveBack()
        {
            _currentIndex = (_items.FindIndex(item => item.Equals(_currentItem)) - 1 + _items.Count) % _items.Count;
            CurrentItem = _items[_currentIndex];
        }
    }
}