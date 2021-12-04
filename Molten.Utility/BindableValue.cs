namespace Molten
{
    public delegate void BindableValueHandler<T>(T oldValue, T newValue);

    /// <summary>A helperclass for detecting when the class object being set is different from the one that was already set.</summary>
    public class BindableValue<T> where T : class
    {
        /// <summary>
        /// Invoked when <see cref="BoundValue"/> changes.
        /// </summary>
        public BindableValueHandler<T> OnBoundValueChanged;

        T _value;
        T _pendingValue;

        /// <summary>
        /// Binds the <see cref="Value"/> to become the <see cref="BoundValue"/>.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// The pending, un-bound value. This will become the <see cref="BoundValue"/> if <see cref="Bind"/> is called.
        /// </summary>
        public T Value
        {
            get => _pendingValue;
            set => _pendingValue = value;
        }

        /// <summary>
        /// The currently bound value. This changes after a call to <see cref="Bind"/>.
        /// </summary>
        public T BoundValue => _value;
    }
}
