using Infrastructure;

namespace UI
{
    public class Limitation
    {
        public ObservableVariable<int> CurrentValue { get; }
        private readonly int _minValue;
        private readonly int _maxValue;

        public Limitation(int minValue, int maxValue)
        {
            _minValue = minValue;
            CurrentValue = new ObservableVariable<int>(minValue);
            _maxValue = maxValue;
        }

        public void Increment()
        {
            if (CurrentValue.Value + 1 > _maxValue)
            {
                CurrentValue.Value = _maxValue;
            }
            else
            {
                CurrentValue.Value += 1;
            }
        }

        public void Decrement()
        {
            if (CurrentValue.Value - 1 < _minValue)
            {
                CurrentValue.Value = _minValue;
            }
            else
            {
                CurrentValue.Value -= 1;
            }
        }

        public void Reset()
        {
            CurrentValue.Value = _minValue;
        }
    }
}