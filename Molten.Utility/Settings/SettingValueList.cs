using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>A utility class for providing basic value change tracking, with subscribable event.</summary>
    /// <typeparam name="T">The type of value to track.</typeparam>
    [DataContract]
    public class SettingValueList<T> : SettingValue
    {
        List<T> _value = new List<T>();
        List<T> _pendingValue = new List<T>();

        /// <summary>Invoked whenever the value changes.</summary>
        public event SubscribedValueHandler<List<T>> OnChanged;

        /// <summary>Performs an implicit conversion from <see cref="SettingValue{T}" /> to <see cref="T" />.</summary>
        /// <param name="val">The value.</param>
        public static implicit operator List<T>(SettingValueList<T> val)
        {
            return val._value;
        }

        protected override void OnDispose()
        {
            _value = null;
            _pendingValue = null;
        }

        /// <summary>Applies the pending value. This will replace the currently-applied value to match the pending one.</summary>
        public override void Apply()
        {
            if (!_value.SequenceEqual(_pendingValue))
            {
                OnChanged?.Invoke(_value, _pendingValue);
                _value.Clear();
                _value.AddRange(_pendingValue);
            }
        }

        /// <summary>Cancels the pending setting and resets it back to the current value.</summary>
        public override void Cancel()
        {
            _pendingValue.Clear();
            _pendingValue.AddRange(_value);
        }

        /// <summary>Gets or sets an existing <see cref="T"/> value in the list.</summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get => _value[index];
            set => _value[index] = value;
        }

        /// <summary>Gets the underlying value list. Any changes will not be applied until <see cref="Apply()"/> is called. </summary>
        [DataMember]
        public List<T> Values
        {
            get { return _pendingValue; }
            set { _pendingValue = value; }
        }
    }
}
