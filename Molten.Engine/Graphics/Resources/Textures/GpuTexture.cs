using Molten.Graphics.Textures;

namespace Molten.Graphics;

/// <summary>
/// A delegate for texture event handlers.
/// </summary>
/// <param name="texture">The texture instance that triggered the event.</param>
public delegate void TextureHandler(GpuTexture texture);

public abstract class GpuTexture : GpuResource, ITexture
{
    /// <summary>
    /// Invoked after resizing of the texture has completed.
    /// </summary>
    public event TextureHandler OnResize;

    TextureDimensions _dimensions;
    GpuResourceFormat _format;

    /// <summary>
    /// Creates a new instance of <see cref="GpuTexture"/>.
    /// </summary>
    /// <param name="device">The <see cref="GpuTexture"/> that the buffer is bound to.</param>
    /// <param name="dimensions">The dimensions of the texture.</param>
    /// <param name="format">The <see cref="GpuResourceFormat"/> of the texture.</param>
    /// <param name="flags">Resource flags which define how the texture can be used.</param>
    /// <param name="name">The name of the texture. This is mainly used for debug purposes.</param>
    /// <exception cref="ArgumentException"></exception>
    protected GpuTexture(GpuDevice device, ref TextureDimensions dimensions, GpuResourceFormat format, GpuResourceFlags flags, string name)
        : base(device, flags)
    {
        if(dimensions.IsCubeMap && dimensions.ArraySize % 6 != 0)
            throw new ArgumentException("The array size of a cube map must be a multiple of 6.", nameof(dimensions.ArraySize));

        LastFrameResizedID = device.Renderer.FrameID;
        ValidateFlags();

        MSAASupport msaaSupport = MSAASupport.NotSupported; // TODO re-support. _renderer.Device.Features.GetMSAASupport(format, aaLevel);
        _dimensions = dimensions;

        Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Width}x{Height}" : name;

        MultiSampleLevel = dimensions.MultiSampleLevel > AntiAliasLevel.Invalid ? dimensions.MultiSampleLevel : AntiAliasLevel.None;
        SampleQuality = msaaSupport != MSAASupport.NotSupported ? dimensions.SampleQuality : MSAAQuality.Default;
        ResourceFormat = format;
    }

    protected void InvokeOnResize()
    {
        OnResize?.Invoke(this);
    }

    protected override void ValidateFlags()
    {
        // Validate RT mip-maps
        if (Flags.Has(GpuResourceFlags.MipMapGeneration))
        {
            if (Flags.Has(GpuResourceFlags.DenyShaderAccess) || !(this is IRenderSurface2D))
                throw new GpuResourceException(this, "Mip-map generation is only available on render-surface shader resources.");
        }

        base.ValidateFlags();
    }

    internal void ResizeTexture(in TextureDimensions newDimensions, GpuResourceFormat newFormat)
    {
        // Avoid resizing/recreation if nothing has actually changed.
        if (_dimensions == newDimensions && ResourceFormat == newFormat)
            return;

        _dimensions = newDimensions;
        ResourceFormat = newFormat;

        OnResizeTexture(in newDimensions, newFormat);
        LastFrameResizedID = Device.Renderer.FrameID;
        OnResize?.Invoke(this);
    }

    protected abstract void OnResizeTexture(ref readonly TextureDimensions dimensions, GpuResourceFormat format);

    /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
    public bool IsBlockCompressed { get; protected set; }

    /// <summary>Gets the width of the texture.</summary>
    public uint Width => _dimensions.Width;

    /// <summary>Gets the height of the texture.</summary>
    public uint Height => _dimensions.Height;

    /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
    public uint Depth => _dimensions.Depth;

    /// <summary>Gets the number of mip map levels in the texture.</summary>
    public uint MipMapCount => _dimensions.MipMapCount;

    /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
    public uint ArraySize => _dimensions.ArraySize;

    /// <summary>
    /// Gets the dimensions of the texture.
    /// </summary>
    public TextureDimensions Dimensions
    {
        get => _dimensions;
        protected set => _dimensions = value;
    }

    /// <inheritdoc/>
    public override ulong SizeInBytes { get; protected set; }

    /// <summary>
    /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
    /// </summary>
    public AntiAliasLevel MultiSampleLevel { get; protected set; }

    /// <summary>
    /// Gets whether or not the texture is multisampled. This is true if <see cref="MultiSampleLevel"/> is at least <see cref="AntiAliasLevel.X2"/>.
    /// </summary>
    public bool IsMultisampled => MultiSampleLevel >= AntiAliasLevel.X2;

    /// <inheritdoc/>
    public MSAAQuality SampleQuality { get; protected set; }

    /// <inheritdoc/>
    public override GpuResourceFormat ResourceFormat
    {
        get => _format;
        protected set
        {
            if (_format != value)
            {
                _format = value;
                IsBlockCompressed = BCHelper.GetBlockCompressed(_format);

                if (IsBlockCompressed)
                    SizeInBytes = BCHelper.GetBCSize(_format, Width, Height, MipMapCount) * ArraySize;
                else
                    SizeInBytes = (ResourceFormat.BytesPerPixel() * (Width * Height)) * ArraySize;
            }
        }
    }
}
