using System;

public class Limitation
{
    public event Action OnCurrentValueUpdate;
    public int CurrentValue { get; private set; }
    private readonly int _minValue;
    private readonly int _maxValue;

    public Limitation(int minValue, int maxValue)
    {
        _minValue = minValue;
        CurrentValue = minValue;
        _maxValue = maxValue;
    }

    public void Increment()
    {
        if (CurrentValue + 1 > _maxValue)
        {
            CurrentValue = _maxValue;
        }
        else
        {
            CurrentValue += 1;
            OnCurrentValueUpdate?.Invoke();
        }
    }

    public void Decrement()
    {
        if (CurrentValue - 1 < _minValue)
        {
            CurrentValue = _minValue;
        }
        else
        {
            CurrentValue -= 1;
            OnCurrentValueUpdate?.Invoke();
        }
    }

    public void Reset()
    {
        CurrentValue = _minValue;
        OnCurrentValueUpdate?.Invoke();
    }
}