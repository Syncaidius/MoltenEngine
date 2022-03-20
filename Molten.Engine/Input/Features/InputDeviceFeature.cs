namespace Molten.Input
{
    public abstract class InputDeviceFeature
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        internal void Update(Timing time)
        {
            OnUpdate(time);
        }

        public abstract void ClearState();

        protected abstract void OnUpdate(Timing time);
    }
}
