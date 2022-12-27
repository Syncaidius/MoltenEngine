namespace Molten.Graphics
{
    public abstract class ContextObject : EngineObject
    {
        internal ContextObject(DeviceDX11 device)
        {
            Device = device; 
        }

        protected override sealed void OnDispose()
        {
            Device.MarkForRelease(this);
        }

        internal abstract void PipelineRelease();

        /// <summary>
        /// Gets the <see cref="Graphics.DeviceDX11"/> that the current <see cref="ContextObject"/> is bound to.
        /// </summary>
        public DeviceDX11 Device { get; }
    }
}
