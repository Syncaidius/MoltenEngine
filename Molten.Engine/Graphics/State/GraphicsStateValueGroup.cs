namespace Molten.Graphics
{
    public class GraphicsStateValueGroup<T>
        where T : class, IGraphicsObject
    {
        GraphicsStateValue<T>[] _values;

        internal GraphicsStateValueGroup(uint capacity)
        {
            _values = new GraphicsStateValue<T>[capacity];
            for (int i = 0; i < _values.Length; i++)
                _values[i] = new GraphicsStateValue<T>();
        }

        public void CopyTo(GraphicsStateValueGroup<T> target)
        {
            for (int i = 0; i < _values.Length; i++)
                _values[i].CopyTo(target._values[i]);
        }

        public bool Bind(GraphicsQueue queue)
        {
            bool changed = false;
            for(int i = 0; i < _values.Length; i++)
            {
                if (_values[i].Bind(queue))
                    changed = true;
            }

            return changed;
        }

        public void Reset()
        {
            for(int i = 0; i < _values.Length; i++)
                _values[i].Value = null;
        }

        /// <summary>
        /// Gets or sets the value of a particular index within the group.
        /// </summary>
        /// <param name="index">The value index.</param>
        /// <returns></returns>
        public GraphicsStateValue<T> this[uint index] => _values[index];

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
                        _values[i].Value = null;
                }

                for(int i = range.Start.Value; i < range.End.Value; i++)
                    _values[i].Value = value[i];
            }
        }

        /// <summary>
        /// Gets the length of the value group.
        /// </summary>
        public uint Length => (uint)_values.Length;
    }
}
