namespace Molten.Graphics
{
    public abstract class ContextObject : EngineObject
    {
        internal ContextObject(Device device)
        {
            Device = device; 
        }

        protected override sealed void OnDispose()
        {
            Device.MarkForRelease(this);
        }

        internal abstract void PipelineRelease();

        /// <summary>
        /// Gets the <see cref="Graphics.Device"/> that the current <see cref="ContextObject"/> is bound to.
        /// </summary>
        public Device Device { get; }
    }
}
