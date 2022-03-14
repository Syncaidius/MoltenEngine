using System;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>A delegate used by <see cref="SettingValue{T}"/> instances when a value changes.</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public delegate void SubscribedValueHandler<T>(T oldValue, T newValue);

    /// <summary>A utility class for providing basic value change tracking, with subscribable event.</summary>
    [DataContract]
    public abstract class SettingValue : EngineObject
    {
        public abstract void Apply();

        public abstract void Cancel();

        public override string ToString()
        {
            return Object != null ? Object.ToString() : base.ToString();
        }

        /// <summary>
        /// Gets the value as an <see cref="object"/>.
        /// </summary>
        public abstract object Object { get; }
    }

    /// <summary>A utility class for providing basic value change tracking, with subscribable event.</summary>
    /// <typeparam name="T">The type of value to track.</typeparam>
    [DataContract]
    public class SettingValue<T> : SettingValue
    {
        T _value;
        T _pendingValue;

        internal void SetSilently(T value)
        {
            _value = value;
            _pendingValue = value;
        }

        protected override void OnDispose()
        {
            _value = default(T);
            _pendingValue = default(T);
        }

        /// <summary>Invoked whenever the value changes.</summary>
        public event SubscribedValueHandler<T> OnChanged;

        /// <summary>Performs an implicit conversion from <see cref="SettingValue{T}" /> to <see cref="T" />.</summary>
        /// <param name="val">The value.</param>
        public static implicit operator T(SettingValue<T> val)
        {
            return val._value;
        }

        public override void Apply()
        {
            if(_value != null)
            {
                if(_pendingValue == null || !_value.Equals(_pendingValue))
                    OnChanged?.Invoke(_value, _pendingValue);
            }
            else
            {
                if(_pendingValue != null)
                    OnChanged?.Invoke(_value, _pendingValue);
            }

            _value = _pendingValue;
        }

        public override void Cancel()
        {
            _pendingValue = _value;
        }

        /// <summary>Gets the current value or sets the pending one. The pending value will not become the current value until <see cref="Apply()"/> is called.</summary>
        [DataMember]
        public T Value
        {
            get { return _pendingValue; }
            set { _pendingValue = value; }
        }

        /// <summary>
        /// Gets the value as an <see cref="object"/>.
        /// </summary>
        public override object Object => _pendingValue;
    }
}
