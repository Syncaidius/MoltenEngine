namespace Molten.Graphics;

/// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
/// <seealso cref="IDisposable" />
public interface ITexture : IGpuResource
{
    /// <summary>
    /// Retrieves the data which makes up the entire texture across all mip-map levels and array slices. The data is returned in a single <see cref="TextureData"/> object.
    /// </summary>
    /// <param name="priority">The priority of the operation.</param>
    /// <param name="callback">The completion callback to trigger once the data has been retrieved from the GPU.</param>
    void GetData(GpuPriority priority, Action<TextureData> callback);

    /// <summary>
    /// Occurs after the <see cref="ITexture"/> is done resizing. Executed by the renderer thread it is bound to.
    /// </summary>
    event TextureHandler OnResize;

    /// <summary>Gets the width of the texture.</summary>
    uint Width { get; }

    /// <summary>Gets the height of the texture.</summary>
    uint Height { get; }

    /// <summary>Gets the depth of the texture.</summary>
    uint Depth { get; }

    /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
    bool IsBlockCompressed { get; }

    /// <summary>Gets the number of mip map levels in the texture.</summary>
    uint MipMapCount { get; }

    /// <summary>Gets the number of array slices in the texture.</summary>
    uint ArraySize { get; }

    /// <summary>
    /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
    /// </summary>
    AntiAliasLevel MultiSampleLevel { get; }

    /// <summary>
    /// Gets the MSAA sample quality level. This is only valid if <see cref="MultiSampleLevel"/> is higher than <see cref="AntiAliasLevel.None"/>.
    /// </summary>
    MSAAQuality SampleQuality { get; }

    /// <summary>
    /// Gets whether or not the texture is multisampled. This is true if <see cref="MultiSampleLevel"/> is higher than <see cref="AntiAliasLevel.None"/>.
    /// </summary>
    bool IsMultisampled { get; }

    /// <summary>
    /// Gets the dimensions of the texture.
    /// </summary>
    TextureDimensions Dimensions { get; }
}
