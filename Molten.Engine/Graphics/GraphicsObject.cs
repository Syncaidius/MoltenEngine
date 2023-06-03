namespace Molten.Graphics
{
    public abstract class GraphicsObject : EngineObject, IGraphicsObject
    {
        protected GraphicsObject(GraphicsDevice device, GraphicsBindTypeFlags bindFlags)
        {
            Device = device;
            BoundTo = new List<GraphicsSlot>();
            BindFlags = bindFlags;
            LastUsedFrameID = device.Queue.Profiler.FrameID;
        }

        /// <summary>
        /// Invoked when the current <see cref="GraphicsObject"/> should apply any changes before being bound to a GPU context.
        /// </summary>
        /// <param name="queue">The <see cref="GraphicsQueue"/> that the current <see cref="GraphicsObject"/> is to be bound to.</param>
        public void Apply(GraphicsQueue queue)
        {
            LastUsedFrameID = queue.Profiler.FrameID;
            OnApply(queue);
        }

        protected abstract void OnApply(GraphicsQueue queue);

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
        /// Gets a list of slots that the current <see cref="GraphicsObject"/> is bound to.
        /// </summary>
        public List<GraphicsSlot> BoundTo { get; }

        /// <summary>
        /// Gets the current binding ID.
        /// </summary>
        public uint BindID { get; set; }

        /// <summary>
        /// Gets or sets the slot bind type of the current <see cref="GraphicsObject"/>.
        /// </summary>
        public GraphicsBindTypeFlags BindFlags { get; set; }

        /// <summary>
        /// Gets the ID of the frame that the current <see cref="GraphicsObject"/> was applied.
        /// </summary>
        public uint LastUsedFrameID { get; private set; }

        /// <summary>
        /// Gets whether or not the current <see cref="GraphicsObject"/> has been successfully disposed and released by its parent <see cref="Device"/>.
        /// </summary>
        public bool IsReleased { get; private set; }
    }
}
