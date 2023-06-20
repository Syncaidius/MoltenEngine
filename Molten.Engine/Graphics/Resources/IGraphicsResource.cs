namespace Molten.Graphics
{
    /// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
    /// <seealso cref="IDisposable" />
    public interface IGraphicsResource : IGraphicsObject
    {
        /// <summary>
        /// Copies the current texture to the destination texture. Both textures must be of the same format and dimensions.
        /// </summary>
        /// <param name="priority">The priority of the copy operation.</param>
        /// <param name="destination">The destination texture.</param>
        /// <param name="completeCallback">A callback to run once the operation has completed.</param>
        void CopyTo(GraphicsPriority priority, GraphicsResource destination, Action<GraphicsResource> completeCallback = null);

        /// <summary>
        /// Invoked when the current <see cref="GraphicsResource"/> should apply any changes before being bound to a GPU context.
        /// </summary>
        /// <param name="cmd">The <see cref="GraphicsQueue"/> that the current <see cref="GraphicsResource"/> is to be bound to.</param>
        void Apply(GraphicsQueue cmd);

        /// <summary>
        /// Gets the <see cref="GraphicsResourceFlags"/> that were provided when the current <see cref="IGraphicsResource"/> was created.
        /// </summary>
        GraphicsResourceFlags Flags { get; }

        /// <summary>
        /// Gets or [protected] sets the <see cref="GraphicsFormat"/> of the resource.
        /// </summary>
        GraphicsFormat ResourceFormat { get; }


        /// <summary>
        /// Gets the <see cref="GraphicsResourceHandle"/> for the current <see cref="IGraphicsResource"/>.
        /// </summary>
        GraphicsResourceHandle Handle { get; }
    }
}
