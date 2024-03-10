namespace Molten.Graphics;

/// <summary>
/// Represents the implementation of a 3D texture.
/// </summary>
public interface ITexture3D : ITexture
{
    /// <summary>
    /// Changes the current texture's dimensions and format.
    /// </summary>
    /// <param name="priority">The priority of the resize operation</param>
    /// <param name="newWidth">The new width.</param>
    /// <param name="newHeight">The new height.</param>
    /// <param name="newMipMapCount">The new mip-map level count.</param>
    /// <param name="newDepth">The new depth of the current <see cref="ITexture3D"/>.</param>
    /// <param name="newFormat">The new graphics format.</param>
    void Resize(GpuPriority priority, uint newWidth, 
        uint newHeight, 
        uint newDepth, 
        uint newMipMapCount = 0, 
        GpuResourceFormat newFormat = GpuResourceFormat.Unknown);
}
