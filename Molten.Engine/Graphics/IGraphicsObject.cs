namespace Molten.Graphics
{
    public interface IGraphicsObject : IDisposable
    {
        /// <summary>
        /// Invoked when the current <see cref="GraphicsObject"/> should apply any changes before being bound to a GPU context.
        /// </summary>
        /// <param name="cmd">The <see cref="GraphicsQueue"/> that the current <see cref="GraphicsObject"/> is to be bound to.</param>
        void Apply(GraphicsQueue cmd);

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> that the current <see cref="GraphicsObject"/> is bound to.
        /// </summary>
        GraphicsDevice Device { get; }

        /// <summary>
        /// Gets the instance-specific version of the current <see cref="GraphicsObject"/>. Any change which will require a device
        /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
        /// </summary>
        uint Version { get; set; }

        /// <summary>
        /// Gets or sets the name of the current <see cref="IGraphicsObject"/>.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets a list of slots that the current <see cref="GraphicsObject"/> is bound to.
        /// </summary>
        List<GraphicsSlot> BoundTo { get; }

        /// <summary>
        /// Gets the current binding ID.
        /// </summary>
        uint BindID { get; set; }

        /// <summary>
        /// Gets or sets the slot bind type of the current <see cref="GraphicsObject"/>.
        /// </summary>
        public GraphicsBindTypeFlags BindFlags { get; set; }

        /// <summary>
        /// Gets the ID of the frame that the current <see cref="GraphicsObject"/> was applied.
        /// </summary>
        uint LastUsedFrameID { get; }

        /// <summary>
        /// Gets whether or not the current <see cref="IGraphicsObject"/> has been successfully disposed and released by its parent <see cref="Device"/>.
        /// </summary>
        bool IsReleased { get; }
    }
}
