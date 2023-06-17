namespace Molten.Graphics
{
    public class GraphicsStateValue<T>
        where T : class, IGraphicsResource
    {
        T _boundValue;
        T _value;
        uint _boundVersion;
        Action<T> _validation;

        public GraphicsStateValue(Action<T> validation = null)
        {
            _validation = validation;
        }

        public void CopyTo(GraphicsStateValue<T> target)
        {
            target._value = _value;
            target._boundValue = _boundValue;
            target._boundVersion = _boundVersion;
        }

        public bool Bind(GraphicsQueue queue)
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

        public T Value
        {
            get => _value;
            set => _value = (value != null && !value.IsReleased) ? value : null;
        }

        public T BoundValue => _boundValue;

        public uint BoundVersion => _boundVersion;
    }
}
