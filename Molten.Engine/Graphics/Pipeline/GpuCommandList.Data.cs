namespace Molten.Graphics;
public partial class GpuCommandList
{
    public void Resize(GpuTexture texture, GpuPriority priority, uint newWidth)
    {
        Resize(texture, priority, newWidth, texture.MipMapCount, texture.ResourceFormat);
    }

    /// <summary>
    /// Resizes the current <see cref="GpuTexture"/>.
    /// </summary>
    /// <param name="texture">The texture to be resized.</param>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="newWidth">The new width.</param>      
    /// <param name="newMipMapCount">The number of mip-map levels per array slice/layer. If set to 0, the current <see cref="MipMapCount"/> will be used.</param>
    /// <param name="newFormat">The new format. If set to <see cref="GpuResourceFormat.Unknown"/>, the existing format will be used.</param>
    public void Resize(GpuTexture texture, GpuPriority priority, uint newWidth, uint newMipMapCount = 0, GpuResourceFormat newFormat = GpuResourceFormat.Unknown)
    {
        Resize(texture, priority, newWidth, texture.Height, texture.ArraySize, newMipMapCount, texture.Depth, newFormat);
    }

    /// <summary>
    /// Resizes the current <see cref="GpuTexture"/>.
    /// </summary>
    /// <param name="texture">The texture to be resized.</param>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="newWidth">The new width.</param>
    /// <param name="newHeight">The new height. If the texture is 1D, height will be defaulted to 1.</param> 
    public void Resize(GpuTexture texture, GpuPriority priority, uint newWidth, uint newHeight)
    {
        Resize(texture, priority, newWidth, newHeight, texture.ArraySize, texture.MipMapCount, texture.Depth, texture.ResourceFormat);
    }

    /// <summary>
    /// Resizes the current <see cref="GpuTexture"/>.
    /// </summary>
    /// <param name="texture">The texture to be resized.</param>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height. If the texture is 1D, height will be defaulted to 1.</param>
    /// <param name="arraySize">For 3D textures, this is the new depth dimension. 
    /// For every other texture type, this is the number of array slices/layers, or the array size.
    /// <para>If set to 0, the existing <see cref="GpuTexture.Depth"/> or <see cref="GpuTexture.ArraySize"/> will be used.</para></param>
    /// <param name="depth">The new depth. Only applicable for 3D textures.</param>
    /// <param name="mipMapCount">The number of mip-map levels per array slice/layer. If set to 0, the current <see cref="GpuTexture.MipMapCount"/> will be used.</param>
    /// <param name="newFormat">The new format. If set to <see cref="GpuResourceFormat.Unknown"/>, the existing format will be used.</param>
    public void Resize(GpuTexture texture, GpuPriority priority, uint width, uint height, uint arraySize = 0, uint mipMapCount = 0, uint depth = 0, GpuResourceFormat newFormat = GpuResourceFormat.Unknown)
    {
        if (texture is ITexture1D)
            height = 1;

        if (texture is not ITexture3D)
            depth = 1;

        TextureResizeTask task = Device.Tasks.Get<TextureResizeTask>();
        task.NewFormat = newFormat == GpuResourceFormat.Unknown ? texture.ResourceFormat : newFormat;
        task.NewDimensions = new TextureDimensions()
        {
            Width = width,
            Height = height,
            ArraySize = arraySize > 0 ? arraySize : texture.ArraySize,
            Depth = depth > 0 ? depth : texture.Depth,
            MipMapCount = mipMapCount > 0 ? mipMapCount : texture.MipMapCount
        };
        Device.Tasks.Push(this, priority, texture, task);
    }

    public unsafe void SetData<T>(GpuTexture texture, GpuPriority priority, ResourceRegion area, T* data, uint numElements, uint bytesPerPixel, uint level, uint arrayIndex = 0,
    GpuTask.EventHandler completeCallback = null)
    where T : unmanaged
    {
        uint texturePitch = area.Width * bytesPerPixel;
        uint pixels = area.Width * area.Height;
        uint expectedBytes = pixels * bytesPerPixel;
        uint dataBytes = (uint)(numElements * sizeof(T));

        if (pixels != numElements)
            throw new Exception($"The provided data does not match the provided area of {area.Width}x{area.Height}. Expected {expectedBytes} bytes. {dataBytes} bytes were provided.");

        // Do a bounds check
        ResourceRegion texBounds = new ResourceRegion(0, 0, 0, texture.Width, texture.Height, texture.Depth);
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
        Device.Tasks.Push(this, priority, texture, task);
    }

    /// <summary>Copies data fom the provided <see cref="TextureData"/> instance into the current texture.</summary>
    /// <param name="data"></param>
    /// <param name="srcMipIndex">The starting mip-map index within the provided <see cref="TextureData"/>.</param>
    /// <param name="srcArraySlice">The starting array slice index within the provided <see cref="TextureData"/>.</param>
    /// <param name="mipCount">The number of mip-map levels to copy per array slice, from the provided <see cref="TextureData"/>.</param>
    /// <param name="arrayCount">The number of array slices to copy from the provided <see cref="TextureData"/>.</param>
    /// <param name="destMipIndex">The mip-map index within the current texture to start copying to.</param>
    /// <param name="destArraySlice">The array slice index within the current texture to start copying to.<</param>
    public unsafe void SetData(GpuTexture texture, GpuPriority priority, TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount,
        uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0, GpuTask.EventHandler completeCallback = null)
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
                SetData(texture, priority, destMip, level.Data, 0, level.TotalBytes, level.Pitch, destSlice, completeCallback);
            }
        }
    }

    public unsafe void SetData(GpuTexture texture, GpuPriority priority, TextureSlice data, uint mipIndex, uint arraySlice, GpuTask.EventHandler completeCallback = null)
    {
        TextureSetTask task = Device.Tasks.Get<TextureSetTask>();
        task.Initialize(data.Data, 1, 0, data.TotalBytes);
        task.Pitch = data.Pitch;
        task.ArrayIndex = arraySlice;
        task.MipLevel = mipIndex;
        task.OnCompleted += completeCallback;
        Device.Tasks.Push(this, priority, texture, task);
    }

    public unsafe void SetData<T>(GpuTexture texture, GpuPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arrayIndex,
        GpuTask.EventHandler completeCallback = null)
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
            Device.Tasks.Push(this, priority, texture, task);
        }
    }

    public unsafe void SetData<T>(GpuTexture texture, GpuPriority priority, ResourceRegion area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0,
        GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        fixed (T* ptrData = data)
            SetData(texture, priority, area, ptrData, (uint)data.Length, bytesPerPixel, level, arrayIndex, completeCallback);
    }

    public unsafe void SetData<T>(GpuTexture texture, GpuPriority priority, uint level, T* data, uint startIndex, uint count, uint pitch, uint arrayIndex,
        GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        TextureSetTask task = Device.Tasks.Get<TextureSetTask>();
        task.Initialize(data, (uint)sizeof(T), startIndex, count);
        task.Pitch = pitch;
        task.ArrayIndex = arrayIndex;
        task.MipLevel = level;
        task.OnCompleted += completeCallback;
        Device.Tasks.Push(this, priority, texture, task);
    }

    public void GetData(GpuTexture texture, GpuPriority priority, Action<TextureData> callback)
    {
        TextureGetTask task = Device.Tasks.Get<TextureGetTask>();
        task.OnGetData = callback;
        Device.Tasks.Push(this, priority, texture, task);
    }

    public void GetData(GpuTexture texture, GpuPriority priority, uint mipLevel, uint arrayIndex, Action<TextureSlice> callback)
    {
        TextureGetSliceTask task = Device.Tasks.Get<TextureGetSliceTask>();
        task.OnGetData = callback;
        task.MipMapLevel = mipLevel;
        task.ArrayIndex = arrayIndex;
        Device.Tasks.Push(this, priority, texture, task);
    }
}
