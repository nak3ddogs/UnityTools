using System;

public class ObservedValue<T>
{
    public event Action<T, T> OnValueChange;

    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            bool isThereChange = _value != null && !_value.Equals(value);
            isThereChange |= (_value != null) != (value != null);

            if (isThereChange)
            {
                var last = _value;
                _value = value;
                OnValueChange?.Invoke(last, _value);
            }
        }
    }

    public ObservedValue(T value = default(T))
    {
        this._value = value;
    }
}
