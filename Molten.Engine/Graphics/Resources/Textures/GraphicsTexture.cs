using Molten.Graphics.Textures;

namespace Molten.Graphics
{
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

        protected GraphicsTexture(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions, AntiAliasLevel aaLevel, 
            MSAAQuality sampleQuality, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) 
            : base(device, flags)
        {
            IsMipMapGenAllowed = allowMipMapGen;
            ValidateFlagCombination();

            MSAASupport msaaSupport = MSAASupport.NotSupported; // TODO re-support. _renderer.Device.Features.GetMSAASupport(format, aaLevel);
            _dimensions = dimensions;
            TextureType = type;
            Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Width}x{Height}" : name;

            MultiSampleLevel = aaLevel > AntiAliasLevel.Invalid ? aaLevel : AntiAliasLevel.None;
            SampleQuality = msaaSupport != MSAASupport.NotSupported ? sampleQuality : MSAAQuality.Default;
            ResourceFormat = format;
            IsBlockCompressed = BCHelper.GetBlockCompressed(format);

            if (IsBlockCompressed)
                SizeInBytes = BCHelper.GetBCSize(format, Width, Height, MipMapCount) * ArraySize;
            else
                SizeInBytes = (ResourceFormat.BytesPerPixel() * (Width * Height)) * ArraySize;
        }

        protected void InvokeOnResize()
        {
            OnResize?.Invoke(this);
        }

        private void ValidateFlagCombination()
        {
            // Validate RT mip-maps
            if (IsMipMapGenAllowed)
            {
                if (Flags.Has(GraphicsResourceFlags.NoShaderAccess) || !(this is IRenderSurface2D))
                    throw new GraphicsResourceException(this, "Mip-map generation is only available on render-surface shader resources.");
            }

            // Only staging resources have CPU-write access.
            if (Flags.Has(GraphicsResourceFlags.CpuWrite))
            {
                if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                    throw new GraphicsResourceException(this, "Staging textures cannot allow shader access. Add GraphicsResourceFlags.NoShaderAccess flag.");

                // Staging buffers cannot have any other flags aside from 
                if (Flags != (GraphicsResourceFlags.CpuWrite | GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite))
                    throw new GraphicsResourceException(this, "Staging textures must have all CPU/GPU read and write flags.");
            }
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
            QueueTask(priority, new TextureResizeTask()
            {
                NewWidth = newWidth,
                NewHeight = Height,
                NewMipMapCount = newMipMapCount,
                NewArraySize = ArraySize,
                NewFormat = newFormat,
            });
        }

        /// <summary>
        /// Resizes the current <see cref="GraphicsTexture"/>.
        /// </summary>
        /// <param name="priority">The priority of the resize operation.</param>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height. If the texture is 1D, height will be defaulted to 1.</param> 
        public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight)
        {
            if( TextureType == GraphicsTextureType.Texture1D)
                newHeight = 1;

            QueueTask(priority, new TextureResizeTask()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewDepth = Depth,
                NewArraySize = ArraySize,
                NewFormat = ResourceFormat,
                NewMipMapCount = MipMapCount,
            });
        }

        /// <summary>
        /// Resizes the current <see cref="GraphicsTexture"/>.
        /// </summary>
        /// <param name="priority">The priority of the resize operation.</param>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height. If the texture is 1D, height will be defaulted to 1.</param>
        /// <param name="newDepthOrArraySize">For 3D textures, this is the new depth dimension. 
        /// For every other texture type, this is the number of array slices/layers, or the array size.
        /// <para>If set to 0, the existing <see cref="Depth"/> or <see cref="ArraySize"/> will be used.</para></param>
        /// <param name="newMipMapCount">The number of mip-map levels per array slice/layer. If set to 0, the current <see cref="MipMapCount"/> will be used.</param>
        /// <param name="newFormat">The new format. If set to <see cref="GraphicsFormat.Unknown"/>, the existing format will be used.</param>
        public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newDepthOrArraySize = 0, uint newMipMapCount = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
        {
            if (TextureType == GraphicsTextureType.Texture1D)
                newHeight = 1;

            TextureResizeTask task = new TextureResizeTask()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount == 0 ? MipMapCount : newMipMapCount,
                NewFormat = newFormat == GraphicsFormat.Unknown ? ResourceFormat : newFormat
            };

            if (TextureType == GraphicsTextureType.Texture3D)
                task.NewDepth = newDepthOrArraySize == 0 ? Depth : newDepthOrArraySize;
            else
                task.NewArraySize = newDepthOrArraySize == 0 ? ArraySize : newDepthOrArraySize;

            QueueTask(priority, task);
        }

        public unsafe void SetData<T>(GraphicsPriority priority, RectangleUI area, T* data, uint numElements, uint bytesPerPixel, uint level, uint arrayIndex = 0,
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
            RectangleUI texBounds = new RectangleUI(0, 0, Width, Height);
            if (!texBounds.Contains(area))
                throw new Exception("The provided area would go outside of the current texture's bounds.");

            QueueTask(priority, new TextureSetTask<T>(data, 0, numElements)
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
                    uint dataID = TextureData.GetLevelID(data.MipMapLevels, mip, slice);
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
            QueueTask(priority, new TextureSetTask<byte>(data.Data, 0, data.TotalBytes)
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
                QueueTask(priority, new TextureSetTask<T>(ptrData, startIndex, count)
                {
                    Pitch = pitch,
                    ArrayIndex = arrayIndex,
                    MipLevel = level,
                    CompleteCallback = completeCallback
                });
            }
        }

        public unsafe void SetData<T>(GraphicsPriority priority, RectangleUI area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0,
            Action<GraphicsResource> completeCallback = null)
            where T : unmanaged
        {
            fixed (T* ptrData = data)
                SetData(priority, area, ptrData, (uint)data.Length, bytesPerPixel, level, arrayIndex, completeCallback);
        }

        public unsafe void SetData<T>(GraphicsPriority priority, uint level, T* data, uint startIndex, uint count, uint pitch, uint arrayIndex, Action<GraphicsResource> completeCallback = null)
            where T : unmanaged
        {
            QueueTask(priority, new TextureSetTask<T>(data, startIndex, count)
            {
                Pitch = pitch,
                ArrayIndex = arrayIndex,
                MipLevel = level,
                CompleteCallback = completeCallback
            });
        }

        public void GetData(GraphicsPriority priority, GraphicsTexture stagingTexture, Action<TextureData> callback)
        {
            QueueTask(priority, new TextureGetTask()
            {
                Staging = stagingTexture,
                CompleteCallback = callback,
            });
        }

        public void GetData(GraphicsPriority priority, GraphicsTexture stagingTexture, uint mipLevel, uint arrayIndex, Action<TextureSlice> callback)
        {
            QueueTask(priority, new TextureGetSliceTask()
            {
                Staging = stagingTexture,
                CompleteCallback = callback,
                ArrayIndex = arrayIndex,
                MipMapLevel = mipLevel,
            });
        }

        internal void OnSetSize(ref TextureResizeTask task)
        {
            // Avoid resizing/recreation if nothing has actually changed.
            if (Width == task.NewWidth &&
                Height == task.NewHeight &&
                Depth == task.NewDepth &&
                MipMapCount == task.NewMipMapCount &&
                ArraySize == task.NewArraySize &&
                ResourceFormat == task.NewFormat)
                return;

            _dimensions.Width = Math.Max(1, task.NewWidth);
            _dimensions.Height = Math.Max(1, task.NewHeight);
            _dimensions.Depth = Math.Max(1, task.NewDepth);
            _dimensions.MipMapLevels = Math.Max(1, task.NewMipMapCount);
            ResourceFormat = task.NewFormat;

            OnSetSize();
            OnResize?.Invoke(this);
        }

        protected abstract void OnSetSize();

        public void CopyTo(GraphicsPriority priority, GraphicsTexture destination, Action<GraphicsResource> completeCallback = null)
        {
            if (ResourceFormat != destination.ResourceFormat)
                throw new ResourceCopyException(this, destination, "The source and destination texture formats do not match.");

            if (!destination.Flags.Has(GraphicsResourceFlags.GpuWrite))
                throw new ResourceCopyException(this, destination, "Cannoy copy to a buffer that does not have GPU-write permission.");

            // Validate dimensions.
            if (destination.Width != Width ||
                destination.Height != Height ||
                destination.Depth != Depth)
                throw new ResourceCopyException(this, destination, "The source and destination textures must have the same dimensions.");

            QueueTask(priority, new ResourceCopyTask()
            {
                Destination = destination,
                CompletionCallback = completeCallback,
            });
        }

        public void CopyTo(GraphicsPriority priority,
            uint sourceLevel, uint sourceSlice,
            GraphicsTexture destination, uint destLevel, uint destSlice,
            Action<GraphicsResource> completeCallback = null)
        {
            if (!destination.Flags.Has(GraphicsResourceFlags.GpuWrite))
                throw new ResourceCopyException(this, destination, "Cannoy copy to a buffer that does not have GPU-write permission.");

            if (ResourceFormat != destination.ResourceFormat)
                throw new ResourceCopyException(this, destination, "The source and destination texture formats do not match.");

            // Validate dimensions.
            // TODO this should only test the source and destination level dimensions, not the textures themselves.
            if (destination.Width != Width ||
                destination.Height != Height ||
                destination.Depth != Depth)
                throw new ResourceCopyException(this, destination, "The source and destination textures must have the same dimensions.");

            if (sourceLevel >= MipMapCount)
                throw new ResourceCopyException(this, destination, "The source mip-map level exceeds the total number of levels in the source texture.");

            if (sourceSlice >= ArraySize)
                throw new ResourceCopyException(this, destination, "The source array slice exceeds the total number of slices in the source texture.");

            if (destLevel >= destination.MipMapCount)
                throw new ResourceCopyException(this, destination, "The destination mip-map level exceeds the total number of levels in the destination texture.");

            if (destSlice >= destination.ArraySize)
                throw new ResourceCopyException(this, destination, "The destination array slice exceeds the total number of slices in the destination texture.");

            QueueTask(priority, new SubResourceCopyTask()
            {
                SrcRegion = null,
                SrcSubResource = (sourceSlice * MipMapCount) + sourceLevel,
                DestResource = destination,
                DestStart = Vector3UI.Zero,
                DestSubResource = (destSlice * destination.MipMapCount) + destLevel,
                CompletionCallback = completeCallback,
            });
        }

        /// <summary>Generates mip maps for the texture via the provided <see cref="GraphicsTexture"/>.</summary>
        public void GenerateMipMaps(GraphicsPriority priority, Action<GraphicsResource> completionCallback = null)
        {
            if (!IsMipMapGenAllowed)
                throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

            QueueTask(priority, new GenerateMipMapsTask()
            {
                OnCompleted = completionCallback
            });
        }

        protected internal abstract void OnGenerateMipMaps(GraphicsQueue cmd);

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        public bool IsBlockCompressed { get; protected set; }

        /// <summary>Gets the width of the texture.</summary>
        public uint Width => _dimensions.Width;

        /// <summary>Gets the height of the texture.</summary>
        public uint Height => _dimensions.Height;

        /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
        public uint Depth => _dimensions.Depth;

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public uint MipMapCount => _dimensions.MipMapLevels;

        /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
        public uint ArraySize => _dimensions.ArraySize;

        /// <summary>
        /// Gets the dimensions of the texture.
        /// </summary>
        public TextureDimensions Dimensions => _dimensions;

        public override uint SizeInBytes { get; }

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
        public override GraphicsFormat ResourceFormat { get; protected set; }

        public GraphicsTextureType TextureType { get; }
    }
}
