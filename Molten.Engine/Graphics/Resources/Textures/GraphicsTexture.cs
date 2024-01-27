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

    protected GraphicsTexture(GraphicsDevice device, ref TextureDimensions dimensions, GraphicsFormat format, GraphicsResourceFlags flags, string name)
        : base(device, flags)
    {
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
        if (Flags.Has(GraphicsResourceFlags.MipMapGeneration))
        {
            if (Flags.Has(GraphicsResourceFlags.DenyShaderAccess) || !(this is IRenderSurface2D))
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
        if (this is ITexture1D)
            height = 1;

        if (this is not ITexture3D)
            depth = 1;

        TextureResizeTask task = Device.Tasks.Get<TextureResizeTask>();
        task.NewFormat = newFormat == GraphicsFormat.Unknown ? ResourceFormat : newFormat;
        task.NewDimensions = new TextureDimensions()
        {
            Width = width,
            Height = height,
            ArraySize = arraySize > 0 ? arraySize : ArraySize,
            Depth = depth > 0 ? depth : Depth,
            MipMapCount = mipMapCount > 0 ? mipMapCount : MipMapCount
        };
        Device.Tasks.Push(priority, this, task);
    }

    public unsafe void SetData<T>(GraphicsPriority priority, ResourceRegion area, T* data, uint numElements, uint bytesPerPixel, uint level, uint arrayIndex = 0,
        GraphicsTask.EventHandler completeCallback = null)
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

        TextureSetTask task = Device.Tasks.Get<TextureSetTask>();
        task.Initialize(data, (uint)sizeof(T), 0, numElements);
        task.Pitch = texturePitch;
        task.StartIndex = 0;
        task.ArrayIndex = arrayIndex;
        task.MipLevel = level;
        task.Area = area;
        task.OnCompleted += completeCallback;
        Device.Tasks.Push(priority, this, task);
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
        uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0, GraphicsTask.EventHandler completeCallback = null)
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

    public unsafe void SetData(GraphicsPriority priority, TextureSlice data, uint mipIndex, uint arraySlice, GraphicsTask.EventHandler completeCallback = null)
    {
        TextureSetTask task = Device.Tasks.Get<TextureSetTask>();
        task.Initialize(data.Data, 1, 0, data.TotalBytes);
        task.Pitch = data.Pitch;
        task.ArrayIndex = arraySlice;
        task.MipLevel = mipIndex;
        task.OnCompleted += completeCallback;
        Device.Tasks.Push(priority, this, task);
    }

    public unsafe void SetData<T>(GraphicsPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arrayIndex,
        GraphicsTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        fixed (T* ptrData = data)
        {
            TextureSetTask task = Device.Tasks.Get<TextureSetTask>();
            task.Initialize(ptrData, (uint)sizeof(T), startIndex, count);
            task.Pitch = pitch;
            task.ArrayIndex = arrayIndex;
            task.MipLevel = level;
            task.OnCompleted += completeCallback;
            Device.Tasks.Push(priority, this, task);
        }
    }

    public unsafe void SetData<T>(GraphicsPriority priority, ResourceRegion area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0,
        GraphicsTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        fixed (T* ptrData = data)
            SetData(priority, area, ptrData, (uint)data.Length, bytesPerPixel, level, arrayIndex, completeCallback);
    }

    public unsafe void SetData<T>(GraphicsPriority priority, uint level, T* data, uint startIndex, uint count, uint pitch, uint arrayIndex,
        GraphicsTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        TextureSetTask task = Device.Tasks.Get<TextureSetTask>();
        task.Initialize(data, (uint)sizeof(T), startIndex, count);
        task.Pitch = pitch;
        task.ArrayIndex = arrayIndex;
        task.MipLevel = level;
        task.OnCompleted += completeCallback;
        Device.Tasks.Push(priority, this, task);
    }

    public void GetData(GraphicsPriority priority, Action<TextureData> callback)
    {
        TextureGetTask task = Device.Tasks.Get<TextureGetTask>();
        task.OnGetData = callback;
        Device.Tasks.Push(priority, this, task);
    }

    public void GetData(GraphicsPriority priority, uint mipLevel, uint arrayIndex, Action<TextureSlice> callback)
    {
        TextureGetSliceTask task = Device.Tasks.Get<TextureGetSliceTask>();
        task.OnGetData = callback;
        task.MipMapLevel = mipLevel;
        task.ArrayIndex = arrayIndex;
        Device.Tasks.Push(priority, this, task);
    }

    internal void ResizeTexture(in TextureDimensions newDimensions, GraphicsFormat newFormat)
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

    protected abstract void OnResizeTexture(ref readonly TextureDimensions dimensions, GraphicsFormat format);

    /// <summary>Generates mip maps for the texture via the current <see cref="GraphicsTexture"/>, if allowed.</summary>
    /// <param name="priority">The priority of the copy operation.</param>
    /// <param name="callback">A callback to run once the operation has completed.</param>
    public void GenerateMipMaps(GraphicsPriority priority, GraphicsTask.EventHandler callback = null)
    {
        if (!Flags.Has(GraphicsResourceFlags.MipMapGeneration))
            throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

        GenerateMipMapsTask task = Device.Tasks.Get<GenerateMipMapsTask>();
        task.OnCompleted += callback;
        Device.Tasks.Push(priority, this, task);
    }

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
    public override GraphicsFormat ResourceFormat
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
