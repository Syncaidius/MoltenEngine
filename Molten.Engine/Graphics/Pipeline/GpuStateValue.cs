namespace Molten.Graphics;

public class GpuStateValue<T>
    where T : class, IGpuResource
{
    T _boundValue;
    T _value;
    uint _boundVersion;
    Action<T> _validation;

    public GpuStateValue(Action<T> validation = null)
    {
        _validation = validation;
    }

    public void CopyTo(GpuStateValue<T> target)
    {
        target._value = _value;
        target._boundValue = _boundValue;
        target._boundVersion = _boundVersion;
    }

    public bool Bind(GpuCommandQueue queue)
    {
        if (_boundValue != _value)
        {
            _boundValue = _value;
            if (_boundValue != null)
            {
                _validation?.Invoke(_boundValue);
                _boundValue.Apply(queue);
                _boundVersion = _boundValue.Version;
            }

            return true;
        }
        else
        {
            if (_boundValue != null)
            {
                _boundValue.Apply(queue);
                if (_boundVersion != _boundValue.Version)
                {
                    _boundVersion = _boundValue.Version;
                    return true;
                }
            }
        }

        return false;
    }


    /// <summary>
    /// Gets the current value. This may not match <see cref="BoundValue"/> until <see cref="Bind(GpuCommandQueue)"/> is called.
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
