namespace Molten.Graphics;

/// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
/// <seealso cref="IDisposable" />
public interface ITexture1D : ITexture
{
    /// <summary>
    /// Resizes a texture to match the specified width, mip-map count and graphics format.
    /// </summary>
    /// <param name="priority">The priority of the copy operation.</param>
    /// <param name="newWidth">The new width.</param>
    /// <param name="newMipMapCount">The new mip-map count.</param>
    /// <param name="format">The new format.</param>
    void Resize(GpuPriority priority, uint newWidth, uint newMipMapCount, GpuResourceFormat format);

    /// <summary>
    /// Resizes a texture to match the specified width.
    /// </summary>
    /// <param name="priority">The priority of the copy operation.</param>
    /// <param name="newWidth">The new width.</param>
    void Resize(GpuPriority priority, uint newWidth);
}
