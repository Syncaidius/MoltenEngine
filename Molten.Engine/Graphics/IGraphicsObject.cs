namespace Molten.Graphics
{
    public interface IGraphicsObject : IDisposable
    {
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
        /// Gets whether or not the current <see cref="IGraphicsObject"/> has been successfully disposed and released by its parent <see cref="Device"/>.
        /// </summary>
        bool IsReleased { get; }
    }
}
