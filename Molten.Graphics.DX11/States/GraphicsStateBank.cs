namespace Molten.Graphics
{
    internal abstract class GraphicsStateBank<T, E> : IDisposable
        where T : GraphicsObject
        where E: struct, IConvertible
    {
        protected T[] _presets;
        List<T> _states;

        internal GraphicsStateBank()
        {
            IConvertible last = ReflectionHelper.GetLastEnumValue<E>();
            int presetArraySize = (int)last + 1;
            _presets = new T[presetArraySize];
            _states = new List<T>();
        }

        public void Dispose()
        {
            foreach (T state in _states)
                state.Dispose();
        }

        protected void AddPreset(E id, T preset)
        {
            int idVal = (int)(object)id;
            _presets[idVal] = preset;
            _states.Add(preset);
        }

        /// <summary>
        /// Attempts to add the provided state to the bank. If an identical state is already stored, the provided one is disposed and the existing one returned. <para/>
        /// The provided state will not be disposed if it is one which is already stored in the bank.
        /// </summary>
        /// <param name="state">The state to add.</param>
        /// <returns></returns>
        internal T AddOrRetrieveExisting(T state)
        {
            foreach (T existing in _states)
            {
                if (existing.Equals(state))
                {
                    if(state != existing)
                        state.Dispose();

                    return existing;
                }
            }            

            _states.Add(state);
            return state;
        }

        internal abstract T GetPreset(E value);
    }
}
