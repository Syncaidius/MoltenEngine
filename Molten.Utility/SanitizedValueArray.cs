using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SanitizedValueArray<T>
    {
        T[] _value;
        bool _dirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SanitizedValueArray{T}"/> class.
        /// </summary>
        /// <param name="size">The size of the value array.</param>
        public SanitizedValueArray(int size)
        {
            _value = new T[size];
        }

        public SanitizedValueArray(T[] existingArray)
        {
            _value = existingArray;
        }

        /// <summary>Resets <see cref="Dirty"/> to false.</summary>
        public void Clean()
        {
            _dirty = false;
        }

        /// <summary>Gets the underlying value array.</summary>
        public T[] UnderlyingArray
        {
            get => _value;
        }

        /// <summary>Gets or sets a value in the array. If the new value is different from the current value at the given index, <see cref="Dirty"/> will be set to true.</summary>
        public T this[int index]
        {
            get => _value[index];
            set
            {
                if (!_value[index].Equals(value))
                {
                    _value[index] = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets the length of the underlying array.
        /// </summary>
        public int Length => _value.Length;

        /// <summary>
        /// Gets a value indicating whether this <see cref="SanitizedValue{T}"/> is dirty.
        /// </summary>
        public bool Dirty => _dirty;
    }
}
