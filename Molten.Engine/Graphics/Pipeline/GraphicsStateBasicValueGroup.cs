namespace Molten.Graphics
{
    public class GraphicsStateBasicValueGroup<T>
        where T : GraphicsObject
    {
        T[] _values;
        T[] _boundValues;
        Action<T, int> _validation;

        public GraphicsStateBasicValueGroup(uint capacity, Action<T, int> validationCallback = null)
        {
            _values = new T[capacity];
            _boundValues = new T[capacity];
            _validation = validationCallback;
        }

        public void CopyTo(GraphicsStateBasicValueGroup<T> target)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                Array.Copy(_values, target._values, _values.Length);
                Array.Copy(_boundValues, target._boundValues, _boundValues.Length);
            }
        }

        public bool Bind(GraphicsQueue queue)
        {
            bool r = false;

            for(int i = 0; i < _values.Length; i++)
            {
                if (_boundValues[i] != _values[i])
                {
                    _boundValues[i] = _values[i];
                    if (_boundValues[i] != null)
                        _validation?.Invoke(_boundValues[i], i);

                    r = true;
                }
            }

            return r;
        }

        public void Reset()
        {
            for(int i = 0; i < _values.Length; i++)
                _values[i] = null;
        }

        /// <summary>
        /// Gets or sets the value of a particular index within the group.
        /// </summary>
        /// <param name="index">The value index.</param>
        /// <returns></returns>
        public T this[uint index]
        {
            get => _values[index];
            set => _values[index] = (value != null && !value.IsReleased) ? value : null;
        }

        /// <summary>
        /// Gets a list of all bound values within the group.
        /// </summary>
        public IReadOnlyList<T> BoundValues => _boundValues;

        /// <summary>
        /// Sets a range of values within the given range.
        /// </summary>
        /// <param name="range">The range of values to be set.</param>
        /// <param name="nullRemaining">If true, any remaining values in the group will be set to null.</param>
        /// <returns></returns>
        public T[] this[Range range, bool nullRemaining = true]
        {
            set
            {
                if (nullRemaining)
                {
                    for (int i = 0; i < _values.Length; i++)
                        _values[i] = null;
                }

                for(int i = range.Start.Value; i < range.End.Value; i++)
                    _values[i] = (value[i] != null && !value[i].IsReleased) ? value[i] : null;
            }
        }

        /// <summary>
        /// Gets the length of the value group.
        /// </summary>
        public int Length => _values.Length;
    }
}
