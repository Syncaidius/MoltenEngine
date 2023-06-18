namespace Molten.Graphics
{
    public abstract class GraphicsObject : EngineObject, IGraphicsObject
    {
        protected GraphicsObject(GraphicsDevice device)
        {
            Device = device;
        }

        protected override sealed void OnDispose()
        {
            Device.MarkForRelease(this);
        }

        internal void GraphicsRelease()
        {
            if (IsReleased)
                throw new GraphicsObjectException(this, "The current GraphicsObject is already released");

            OnGraphicsRelease();
            IsReleased = true;
        }

        /// <summary>
        /// Invoked when the object should release any graphics resources.
        /// </summary>
        protected abstract void OnGraphicsRelease();

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> that the current <see cref="GraphicsObject"/> is bound to.
        /// </summary>
        public GraphicsDevice Device { get; }

        /// <summary>
        /// Gets the instance-specific version of the current <see cref="GraphicsObject"/>. Any change which will require a device
        /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets whether or not the current <see cref="GraphicsObject"/> has been successfully disposed and released by its parent <see cref="Device"/>.
        /// </summary>
        public bool IsReleased { get; private set; }

        /// <summary>
        /// Gets the frame ID that the current <see cref="GraphicsObject"/> was initially marked for release.
        /// </summary>
        internal ulong ReleaseFrameID { get; set; }
    }
}
