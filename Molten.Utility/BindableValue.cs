namespace Molten
{
    public delegate void BindableValueHandler<T>(T oldValue, T newValue);

    /// <summary>A helperclass for detecting when the class object being set is different from the one that was already set.</summary>
    public class BindableValue<T> where T : class
    {
        public BindableValueHandler<T> OnBoundValueChanged;

        T _value;
        T _pendingValue;

        public bool Bind()
        {
            if (_value != _pendingValue)
            {
                OnBoundValueChanged?.Invoke(_value, _pendingValue);
                _value = _pendingValue;
                return true;
            }

            return false;
        }

        public T Value
        {
            get => _pendingValue;
            set => _pendingValue = value;
        }

        public T BoundValue => _value;
    }
}
