namespace Molten.Graphics;

/// <summary>
/// A component class that is intended for use by a <see cref="GpuState"/> for binding a <see cref="GpuObject"/> to a slot or pipeline element.
/// </summary>
/// <typeparam name="T"></typeparam>
public class GpuStateBasicValue<T>
    where T : GpuObject
{
    T _boundValue;
    T _value;
    Action<T> _validation;

    public GpuStateBasicValue(Action<T> validation = null)
    {
        _validation = validation;
    }

    public void CopyTo(GpuStateBasicValue<T> target)
    {
        target._value = _value;
        target._boundValue = _boundValue;
    }

    public bool Bind()
    {
        if (_boundValue != _value)
        {
            _boundValue = _value;
            if (_boundValue != null)
                _validation?.Invoke(_boundValue);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the current value. This may not match <see cref="BoundValue"/> until <see cref="Bind()"/> is called.
    /// </summary>
    public T Value
    {
        get => _value;
        set => _value = (value != null && !value.IsReleased) ? value : null;
    }

    /// <summary>
    /// Gets the currently-bound <typeparamref name="T"/> instance.
    /// </summary>
    public T BoundValue => _boundValue;
}
