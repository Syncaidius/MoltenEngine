using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>A helper class for updating a value in an array with some sanitization assistance.</summary>
    /// <typeparam name="T"></typeparam>
    public class SatitizedArrayValue<T>
    {
        T _value;
        bool _dirty;

        /// <summary>Resets <see cref="Dirty"/> to false.</summary>
        public void Clean()
        {
            _dirty = false;
        }

        /// <summary>Gets or sets the value. If the new value is different from the current value, <see cref="Dirty"/> will be set to true.</summary>
        public T Value
        {
            get => _value;
            set
            {
                if (!_value.Equals(value))
                {
                    _value = value;
                    _dirty = true;
                }
            }
        }
    }
}
