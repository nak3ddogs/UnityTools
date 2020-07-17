using System;

public class ObservedValue<T>
{
	public event Action<T> OnValueChange;
	private T _value;

	public T Value
	{
		get => _value;
		set
		{
			if (!_value.Equals(value)) OnValueChange.Invoke(value);
			_value = value;
		}
	}

	public ObservedValue(T value = default(T))
	{
		this._value = value;
	}
}