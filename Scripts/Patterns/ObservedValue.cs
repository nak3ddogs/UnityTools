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
			bool isThereChange = !_value.Equals(value);
			_value = value;
			if (isThereChange) OnValueChange.Invoke(_value);
		}
	}

	public ObservedValue(T value = default(T))
	{
		this._value = value;
	}

	public void Listen(ObservedValue<T> source)
	{
		this.Value = source.Value;
		source.OnValueChange += x => this.Value = x;
	}
}