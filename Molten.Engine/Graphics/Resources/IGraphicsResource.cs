namespace Molten.Graphics;

/// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
/// <seealso cref="IDisposable" />
public interface IGraphicsResource : IDisposable
{
    /// <summary>
    /// Copies the current texture to the destination texture. Both textures must be of the same format and dimensions.
    /// </summary>
    /// <param name="priority">The priority of the copy operation.</param>
    /// <param name="destination">The destination texture.</param>
    /// <param name="completeCallback">A callback to run once the operation has completed.</param>
    void CopyTo(GraphicsPriority priority, GraphicsResource destination, Action<GraphicsResource> completeCallback = null);

    /// <summary>
    /// Copies the current texture to the destination texture. Both texture levels must be of the same format and dimensions.
    /// </summary>
    /// <param name="priority">The priority of the copy operation.</param>
    /// <param name="sourceLevel">The source mip-map level.</param>
    /// <param name="sourceSlice">The source array slice.</param>
    /// <param name="destination">The destination resource.</param>
    /// <param name="destLevel">The destination mip-map level.</param>
    /// <param name="destSlice">The destination array slice.</param>
    /// <param name="completeCallback">A callback to run once the operation has completed.</param>
    void CopyTo(GraphicsPriority priority,
        uint sourceLevel, uint sourceSlice,
        GraphicsResource destination, uint destLevel, uint destSlice,
        Action<GraphicsResource> completeCallback = null);

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

    /// <summary>
    /// Gets the <see cref="GraphicsDevice"/> that the current <see cref="IGraphicsResource"/> is bound to.
    /// </summary>
    GraphicsDevice Device { get; }

    /// <summary>
    /// Gets the instance-specific version of the current <see cref="GraphicsObject"/>. Any change which will require a device
    /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
    /// </summary>
    uint Version { get; set; }

    /// <summary>
    /// Gets or sets the name of the current <see cref="IGraphicsResource"/>.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets whether or not the current <see cref="IGraphicsResource"/> has been successfully disposed and released by its parent <see cref="Device"/>.
    /// </summary>
    bool IsReleased { get; }

    /// <summary>
    /// Gets the unique engine object ID (EOID) of the current <see cref="IGraphicsObject"/>.
    /// </summary>
    public ulong EOID { get; }
}
