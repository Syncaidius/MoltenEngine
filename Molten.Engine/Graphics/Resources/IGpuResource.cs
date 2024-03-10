namespace Molten.Graphics;

/// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
/// <seealso cref="IDisposable" />
public interface IGpuResource : IDisposable
{
    /// <summary>
    /// Copies the current texture to the destination texture. Both textures must be of the same format and dimensions.
    /// </summary>
    /// <param name="priority">The priority of the copy operation.</param>
    /// <param name="destination">The destination texture.</param>
    /// <param name="completeCallback">A callback to run once the operation has completed.</param>
    void CopyTo(GpuPriority priority, GpuResource destination, GraphicsTask.EventHandler completeCallback = null);

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
    void CopyTo(GpuPriority priority,
        uint sourceLevel, uint sourceSlice,
        GpuResource destination, uint destLevel, uint destSlice,
        GraphicsTask.EventHandler completeCallback = null);

    /// <summary>
    /// Invoked when the current <see cref="GpuResource"/> should apply any changes before being bound to a GPU context.
    /// </summary>
    /// <param name="cmd">The <see cref="GpuCommandList"/> that the current <see cref="GpuResource"/> is to be bound to.</param>
    void Apply(GpuCommandList cmd);

    /// <summary>
    /// Gets the <see cref="GpuResourceFlags"/> that were provided when the current <see cref="IGpuResource"/> was created.
    /// </summary>
    GpuResourceFlags Flags { get; }

    /// <summary>
    /// Gets or [protected] sets the <see cref="GpuResourceFormat"/> of the resource.
    /// </summary>
    GpuResourceFormat ResourceFormat { get; }

    /// <summary>
    /// Gets the <see cref="GpuResourceHandle"/> for the current <see cref="IGpuResource"/>.
    /// </summary>
    GpuResourceHandle Handle { get; }

    /// <summary>
    /// Gets the <see cref="GpuDevice"/> that the current <see cref="IGpuResource"/> is bound to.
    /// </summary>
    GpuDevice Device { get; }

    /// <summary>
    /// Gets the instance-specific version of the current <see cref="GpuObject"/>. Any change which will require a device
    /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
    /// </summary>
    uint Version { get; set; }

    /// <summary>
    /// Gets or sets the name of the current <see cref="IGpuResource"/>.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets whether or not the current <see cref="IGpuResource"/> has been successfully disposed and released by its parent <see cref="Device"/>.
    /// </summary>
    bool IsReleased { get; }

    /// <summary>
    /// Gets the unique engine object ID (EOID) of the current <see cref="GpuObject"/>.
    /// </summary>
    public ulong EOID { get; }
}
