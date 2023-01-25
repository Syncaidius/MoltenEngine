namespace Molten.Graphics
{
    public class ShaderStateBank<T> : EngineObject
        where T : GraphicsObject
    {
        T[] _states;

        public ShaderStateBank()
        {
            _states = new T[(int)StateConditions.All + 1];
        }

        public void FillMissingWith(T state)
        {
            for (int i = 0; i < _states.Length; i++)
                _states[i] = _states[i] ?? state;
        }

        public void FillMissingWith(ShaderStateBank<T> source)
        {
            for (int i = 0; i < _states.Length; i++)
                _states[i] = _states[i] ?? source._states[i];
        }

        protected override void OnDispose()
        {
            for (int i = 0; i < _states.Length; i++)
                _states[i].Dispose();
        }

        public T this[StateConditions conditions]
        {
            get => _states[(int)conditions];
            set => _states[(int)conditions] = value;
        }
    }
}
