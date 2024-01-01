using Molten.Graphics.Textures;

namespace Molten.Graphics;

/// <summary>
/// A delegate for texture event handlers.
/// </summary>
/// <param name="texture">The texture instance that triggered the event.</param>
public delegate void TextureHandler(GraphicsTexture texture);

public abstract class GraphicsTexture : GraphicsResource, ITexture
{            
    /// <summary>
    /// Invoked after resizing of the texture has completed.
    /// </summary>
    public event TextureHandler OnResize;

    TextureDimensions _dimensions;
    GraphicsFormat _format;

    protected GraphicsTexture(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions, AntiAliasLevel aaLevel, 
        MSAAQuality sampleQuality, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) 
        : base(device, flags)
    {
        LastFrameResizedID = device.Renderer.FrameID;
        IsMipMapGenAllowed = allowMipMapGen;
        ValidateFlags();

        MSAASupport msaaSupport = MSAASupport.NotSupported; // TODO re-support. _renderer.Device.Features.GetMSAASupport(format, aaLevel);
        _dimensions = dimensions;
        TextureType = type;

        Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Width}x{Height}" : name;

        MultiSampleLevel = aaLevel > AntiAliasLevel.Invalid ? aaLevel : AntiAliasLevel.None;
        SampleQuality = msaaSupport != MSAASupport.NotSupported ? sampleQuality : MSAAQuality.Default;
        ResourceFormat = format;
    }

    protected void InvokeOnResize()
    {
        OnResize?.Invoke(this);
    }

    protected override void ValidateFlags()
    {
        // Validate RT mip-maps
        if (IsMipMapGenAllowed)
        {
            if (Flags.Has(GraphicsResourceFlags.NoShaderAccess) || !(this is IRenderSurface2D))
                throw new GraphicsResourceException(this, "Mip-map generation is only available on render-surface shader resources.");
        }

        base.ValidateFlags();
    }

public void Resize(GraphicsPriority priority, uint newWidth)
    {
        Resize(priority, newWidth, MipMapCount, ResourceFormat);
    }

    /// <summary>
    /// Resizes the current <see cref="GraphicsTexture"/>.
    /// </summary>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="newWidth">The new width.</param>      
    /// <param name="newMipMapCount">The number of mip-map levels per array slice/layer. If set to 0, the current <see cref="MipMapCount"/> will be used.</param>
    /// <param name="newFormat">The new format. If set to <see cref="GraphicsFormat.Unknown"/>, the existing format will be used.</param>
    public void Resize(GraphicsPriority priority, uint newWidth, uint newMipMapCount = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
    {
        Resize(priority, newWidth, Height, ArraySize, newMipMapCount, Depth, newFormat);
    }

    /// <summary>
    /// Resizes the current <see cref="GraphicsTexture"/>.
    /// </summary>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="newWidth">The new width.</param>
    /// <param name="newHeight">The new height. If the texture is 1D, height will be defaulted to 1.</param> 
    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight)
    {
        Resize(priority, newWidth, newHeight, ArraySize, MipMapCount, Depth, ResourceFormat);
    }

    /// <summary>
    /// Resizes the current <see cref="GraphicsTexture"/>.
    /// </summary>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height. If the texture is 1D, height will be defaulted to 1.</param>
    /// <param name="arraySize">For 3D textures, this is the new depth dimension. 
    /// For every other texture type, this is the number of array slices/layers, or the array size.
    /// <para>If set to 0, the existing <see cref="Depth"/> or <see cref="ArraySize"/> will be used.</para></param>
    /// <param name="depth">The new depth. Only applicable for 3D textures.</param>
    /// <param name="mipMapCount">The number of mip-map levels per array slice/layer. If set to 0, the current <see cref="MipMapCount"/> will be used.</param>
    /// <param name="newFormat">The new format. If set to <see cref="GraphicsFormat.Unknown"/>, the existing format will be used.</param>
    public void Resize(GraphicsPriority priority, uint width, uint height, uint arraySize = 0, uint mipMapCount = 0, uint depth = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
    {
        if (TextureType == GraphicsTextureType.Texture1D)
            height = 1;

        if (TextureType != GraphicsTextureType.Texture3D || TextureType != GraphicsTextureType.Surface3D)
            depth = 1;

        Device.Renderer.PushTask(priority, this, new TextureResizeTask()
        {
            NewFormat = newFormat == GraphicsFormat.Unknown ? ResourceFormat : newFormat,
            NewDimensions = new TextureDimensions()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize > 0 ? arraySize : ArraySize,
                Depth = depth > 0 ? depth : Depth,
                MipMapCount = mipMapCount > 0 ? mipMapCount : MipMapCount
            },
        });
    }

    public unsafe void SetData<T>(GraphicsPriority priority, ResourceRegion area, T* data, uint numElements, uint bytesPerPixel, uint level, uint arrayIndex = 0,
        Action<GraphicsResource> completeCallback = null)
        where T : unmanaged
    {
        uint texturePitch = area.Width * bytesPerPixel;
        uint pixels = area.Width * area.Height;
        uint expectedBytes = pixels * bytesPerPixel;
        uint dataBytes = (uint)(numElements * sizeof(T));

        if (pixels != numElements)
            throw new Exception($"The provided data does not match the provided area of {area.Width}x{area.Height}. Expected {expectedBytes} bytes. {dataBytes} bytes were provided.");

        // Do a bounds check
        ResourceRegion texBounds = new ResourceRegion(0, 0, 0, Width, Height, Depth);
        if (!texBounds.Contains(area))
            throw new Exception("The provided area would go outside of the current texture's bounds.");

        Device.Renderer.PushTask(priority, this, new TextureSetTask(data, (uint)sizeof(T), 0, numElements)
        {
            Pitch = texturePitch,
            StartIndex = 0,
            ArrayIndex = arrayIndex,
            MipLevel = level,
            Area = area,
            CompleteCallback = completeCallback,
        });
    }

    /// <summary>Copies data fom the provided <see cref="TextureData"/> instance into the current texture.</summary>
    /// <param name="data"></param>
    /// <param name="srcMipIndex">The starting mip-map index within the provided <see cref="TextureData"/>.</param>
    /// <param name="srcArraySlice">The starting array slice index within the provided <see cref="TextureData"/>.</param>
    /// <param name="mipCount">The number of mip-map levels to copy per array slice, from the provided <see cref="TextureData"/>.</param>
    /// <param name="arrayCount">The number of array slices to copy from the provided <see cref="TextureData"/>.</param>
    /// <param name="destMipIndex">The mip-map index within the current texture to start copying to.</param>
    /// <param name="destArraySlice">The array slice index within the current texture to start copying to.<</param>
    public unsafe void SetData(GraphicsPriority priority, TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount,
        uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0, Action<GraphicsResource> completeCallback = null)
    {
        TextureSlice level = null;
        for (uint a = 0; a < arrayCount; a++)
        {
            for (uint m = 0; m < mipCount; m++)
            {
                uint slice = srcArraySlice + a;
                uint mip = srcMipIndex + m;
                uint dataID = data.GetLevelID(mip, slice);
                level = data.Levels[dataID];

                if (level.TotalBytes == 0)
                    continue;

                uint destSlice = destArraySlice + a;
                uint destMip = destMipIndex + m;
                SetData(priority, destMip, level.Data, 0, level.TotalBytes, level.Pitch, destSlice, completeCallback);
            }
        }
    }

    public unsafe void SetData(GraphicsPriority priority, TextureSlice data, uint mipIndex, uint arraySlice, Action<GraphicsResource> completeCallback = null)
    {
        // Store pending change.
        Device.Renderer.PushTask(priority, this, new TextureSetTask(data.Data, 1, 0, data.TotalBytes)
        {
            Pitch = data.Pitch,
            ArrayIndex = arraySlice,
            MipLevel = mipIndex,
            CompleteCallback = completeCallback,
        });
    }

    public unsafe void SetData<T>(GraphicsPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arrayIndex, 
        Action<GraphicsResource> completeCallback = null)
        where T : unmanaged
    {
        fixed (T* ptrData = data)
        {
            Device.Renderer.PushTask(priority, this, new TextureSetTask(ptrData, (uint)sizeof(T), startIndex, count)
            {
                Pitch = pitch,
                ArrayIndex = arrayIndex,
                MipLevel = level,
                CompleteCallback = completeCallback
            });
        }
    }

    public unsafe void SetData<T>(GraphicsPriority priority, ResourceRegion area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0,
        Action<GraphicsResource> completeCallback = null)
        where T : unmanaged
    {
        fixed (T* ptrData = data)
            SetData(priority, area, ptrData, (uint)data.Length, bytesPerPixel, level, arrayIndex, completeCallback);
    }

    public unsafe void SetData<T>(GraphicsPriority priority, uint level, T* data, uint startIndex, uint count, uint pitch, uint arrayIndex, Action<GraphicsResource> completeCallback = null)
        where T : unmanaged
    {
        Device.Renderer.PushTask(priority, this, new TextureSetTask(data, (uint)sizeof(T), startIndex, count)
        {
            Pitch = pitch,
            ArrayIndex = arrayIndex,
            MipLevel = level,
            CompleteCallback = completeCallback
        });
    }

    public void GetData(GraphicsPriority priority, GraphicsTexture stagingTexture, Action<TextureData> callback)
    {
        Device.Renderer.PushTask(priority, this, new TextureGetTask()
        {
            Staging = stagingTexture,
            CompleteCallback = callback,
        });
    }

    public void GetData(GraphicsPriority priority, GraphicsTexture stagingTexture, uint mipLevel, uint arrayIndex, Action<TextureSlice> callback)
    {
        Device.Renderer.PushTask(priority, this, new TextureGetSliceTask()
        {
            Staging = stagingTexture,
            CompleteCallback = callback,
            ArrayIndex = arrayIndex,
            MipMapLevel = mipLevel,
        });
    }

    internal void ResizeTexture(in TextureDimensions newDimensions, GraphicsFormat newFormat)
    {
        // Avoid resizing/recreation if nothing has actually changed.
        if (_dimensions == newDimensions && ResourceFormat == newFormat)
            return;

        _dimensions = newDimensions;
        ResourceFormat = newFormat;

        // Cap frame-buffer size to 1 if the resource is static.
        uint fbSize = 1;
        uint fbIndex = 0;

        if (Flags.Has(GraphicsResourceFlags.Buffered))
        {
            fbSize = GetMaxFrameBufferSize(Device.FrameBufferSize);
            fbIndex = Math.Min(fbSize - 1, Device.FrameBufferIndex);
        }

        OnResizeTexture(in newDimensions, newFormat, fbSize, fbIndex, Device.Renderer.FrameID);
        LastFrameResizedID = Device.Renderer.FrameID;
        OnResize?.Invoke(this);
    }

    protected abstract void OnResizeTexture(in TextureDimensions dimensions, GraphicsFormat format, uint frameBufferSize, uint frameBufferIndex, ulong frameID);

    /// <summary>Generates mip maps for the texture via the provided <see cref="GraphicsTexture"/>.</summary>
    public void GenerateMipMaps(GraphicsPriority priority, Action<GraphicsResource> completionCallback = null)
    {
        if (!IsMipMapGenAllowed)
            throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

        Device.Renderer.PushTask(priority, this, new GenerateMipMapsTask()
        {
            OnCompleted = completionCallback
        });
    }

    protected internal abstract void OnGenerateMipMaps(GraphicsQueue queue);

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

    public override uint SizeInBytes { get; protected set; }

    /// <summary>
    /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
    /// </summary>
    public AntiAliasLevel MultiSampleLevel { get; protected set; }

    /// <summary>
    /// Gets whether or not the texture is multisampled. This is true if <see cref="MultiSampleLevel"/> is at least <see cref="AntiAliasLevel.X2"/>.
    /// </summary>
    public bool IsMultisampled => MultiSampleLevel >= AntiAliasLevel.X2;

    public MSAAQuality SampleQuality { get; protected set; }

    public bool IsMipMapGenAllowed { get; }

    /// <inheritdoc/>
    public override GraphicsFormat ResourceFormat
    {
        get => _format;
        protected set
        {
            if(_format != value)
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

    public GraphicsTextureType TextureType { get; }
}
