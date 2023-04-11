namespace Molten.Graphics
{
    /// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
    /// <seealso cref="IDisposable" />
    public interface IGraphicsResource : IGraphicsObject
    {
        /// <summary>
        /// Gets the <see cref="GraphicsResourceFlags"/> that were provided when the current <see cref="IGraphicsResource"/> was created.
        /// </summary>
        GraphicsResourceFlags Flags { get; }

        /// <summary>
        /// Gets or [protected] sets the <see cref="GraphicsFormat"/> of the resource.
        /// </summary>
        GraphicsFormat ResourceFormat { get; }
    }
}
