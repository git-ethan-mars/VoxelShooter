using System;

namespace Infrastructure
{
    public class ObservableVariable<T>
    {
        public event Action<T> ValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                ValueChanged?.Invoke(value);
            }
        }

        private T _value;

        public ObservableVariable(T value)
        {
            _value = value;
        }
    }
}