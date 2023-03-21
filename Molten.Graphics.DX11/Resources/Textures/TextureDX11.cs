using Molten.Collections;
using Molten.Graphics.Textures;
using Molten.UI;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public delegate void TextureEvent(TextureDX11 texture);

    public unsafe abstract partial class TextureDX11 : ResourceDX11, ITexture
    {
        /// <summary>Triggered right before the internal texture resource is created.</summary>
        public event TextureEvent OnPreCreate;

        /// <summary>Triggered after the internal texture resource has been created.</summary>
        public event TextureEvent OnCreate;

        /// <summary>Triggered if the creation of the internal texture resource has failed (resulted in a null resource).</summary>
        public event TextureEvent OnCreateFailed;

        /// <summary>
        /// Invoked after resizing of the texture has completed.
        /// </summary>
        public event TextureHandler OnResize;

        ID3D11Resource* _native;

        internal TextureDX11(RenderService renderer, uint width, uint height, uint depth, uint mipCount, 
            uint arraySize, AntiAliasLevel aaLevel, MSAAQuality sampleQuality, Format format, TextureFlags flags, string name) : base(renderer.Device as DeviceDX11,
                ((flags & TextureFlags.AllowUAV) == TextureFlags.AllowUAV ? GraphicsBindTypeFlags.Output : GraphicsBindTypeFlags.None) |
                ((flags & TextureFlags.SharedResource) == TextureFlags.SharedResource ? GraphicsBindTypeFlags.Input : GraphicsBindTypeFlags.None))
        {
            Renderer = renderer;
            Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{width}x{height}" : name;
            AccessFlags = flags;
            ValidateFlagCombination();

            MSAASupport msaaSupport = MSAASupport.NotSupported; // TODO re-support. _renderer.Device.Features.GetMSAASupport(format, aaLevel);

            Flags = GraphicsResourceFlags.None; // TODO refactor textures to use GraphicsResourceFlags.
            Width = width;
            Height = height;
            Depth = depth;
            MipMapCount = mipCount;
            ArraySize = arraySize;
            MultiSampleLevel = aaLevel > AntiAliasLevel.Invalid ? aaLevel : AntiAliasLevel.None;
            SampleQuality = msaaSupport != MSAASupport.NotSupported ? sampleQuality : MSAAQuality.Default;
            DxgiFormat = format;
            IsValid = false;

            IsBlockCompressed = BCHelper.GetBlockCompressed(DxgiFormat.FromApi());

            if (IsBlockCompressed)
                SizeInBytes = BCHelper.GetBCSize(DataFormat, width, height, mipCount) * arraySize;
            else
                SizeInBytes = (DataFormat.BytesPerPixel() * (width * height)) * arraySize;
        }

        private void ValidateFlagCombination()
        {
            // Validate RT mip-maps
            if (HasFlags(TextureFlags.AllowMipMapGeneration))
            {
                if(HasFlags(TextureFlags.NoShaderResource) || !(this is RenderSurface2DDX11))
                    throw new TextureFlagException(AccessFlags, "Mip-map generation is only available on render-surface shader resources.");
            }

            if (HasFlags(TextureFlags.Staging))
            {
                if (AccessFlags != (TextureFlags.Staging) && AccessFlags != (TextureFlags.Staging | TextureFlags.NoShaderResource))
                    throw new TextureFlagException(AccessFlags, "Staging textures cannot have other flags set except NoShaderResource.");

                AccessFlags |= TextureFlags.NoShaderResource;
            }
        }

        protected BindFlag GetBindFlags()
        {
            BindFlag result = 0;

            if (HasFlags(TextureFlags.AllowUAV))
                result |= BindFlag.UnorderedAccess;

            if (!HasFlags(TextureFlags.NoShaderResource))
                result |= BindFlag.ShaderResource;

            if (this is RenderSurface2DDX11)
                result |= BindFlag.RenderTarget;

            if (this is DepthSurfaceDX11)
                result |= BindFlag.DepthStencil;

            return result;
        }

        protected ResourceMiscFlag GetResourceFlags()
        {
            ResourceMiscFlag result = 0;

            if (HasFlags(TextureFlags.SharedResource))
                result |= ResourceMiscFlag.Shared;

            if (HasFlags(TextureFlags.AllowMipMapGeneration))
                result |= ResourceMiscFlag.GenerateMips;

            return result;
        }

        protected Usage GetUsageFlags()
        {
            if (HasFlags(TextureFlags.Staging))
                return Usage.Staging;
            else if (HasFlags(TextureFlags.Dynamic))
                return Usage.Dynamic;
            else
                return Usage.Default;
        }

        protected CpuAccessFlag GetAccessFlags()
        {
            if (HasFlags(TextureFlags.Staging))
                return CpuAccessFlag.Read;
            else if (HasFlags(TextureFlags.Dynamic))
                return CpuAccessFlag.Write;
            else
                return 0;
        }

        protected void CreateTexture(bool resize)
        {
            OnPreCreate?.Invoke(this);

            // Dispose of old resources
            OnDisposeForRecreation();
            _native = CreateResource(resize);
            SetDebugName(Name);

            if (_native != null)
            {
                if (!HasFlags(TextureFlags.NoShaderResource))
                {
                    SetSRVDescription(ref SRV.Desc);
                    SRV.Create(_native);
                    SRV.SetDebugName($"{Name}_SRV");
                }

                if (HasFlags(TextureFlags.AllowUAV))
                {
                    SetUAVDescription(ref SRV.Desc, ref UAV.Desc);
                    UAV.Create(_native);
                    SRV.SetDebugName($"{Name}_UAV");
                }

                Version++;
                OnCreate?.Invoke(this);
            }
            else
            {
                OnCreateFailed?.Invoke(this);
            }

            IsValid = _native != null;
        }

        protected abstract void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc);

        protected abstract void SetSRVDescription(ref ShaderResourceViewDesc1 desc);

        protected virtual void OnDisposeForRecreation()
        {
            GraphicsRelease();
        }

        public override void GraphicsRelease()
        {
            base.GraphicsRelease();

            //TrackDeallocation();
            SilkUtil.ReleasePtr(ref _native);
        }

        public bool HasFlags(TextureFlags flags)
        {
            return (AccessFlags & flags) == flags;
        }

        /// <summary>Generates mip maps for the texture via the provided <see cref="CommandQueueDX11"/>.</summary>
        public void GenerateMipMaps(GraphicsPriority priority)
        {
            if (!((AccessFlags & TextureFlags.AllowMipMapGeneration) == TextureFlags.AllowMipMapGeneration))
                throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

            QueueTask(priority, new GenerateMipMapsTask());
        }

        public void SetData<T>(GraphicsPriority priority, RectangleUI area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0, 
            Action<GraphicsResource> completeCallback = null)
            where T : unmanaged
        {
            fixed (T* ptrData = data)
                SetData(priority, area, ptrData, (uint)data.Length, bytesPerPixel, level, arrayIndex, completeCallback);
        }

        public void SetData<T>(GraphicsPriority priority, RectangleUI area, T* data, uint numElements, uint bytesPerPixel, uint level, uint arrayIndex = 0, 
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

            QueueTask(priority, new TextureSet<T>(data, 0, numElements)
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
        public void SetData(GraphicsPriority priority, TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount,
            uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0, Action<GraphicsResource> completeCallback = null)
        {
            TextureSlice level = null;
            for(uint a = 0; a < arrayCount; a++)
            {
                for(uint m = 0; m < mipCount; m++)
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

        public void SetData(GraphicsPriority priority, TextureSlice data, uint mipIndex, uint arraySlice, Action<GraphicsResource> completeCallback = null)
        {
            // Store pending change.
            QueueTask(priority, new TextureSet<byte>(data.Data, 0, data.TotalBytes)
            {
                Pitch = data.Pitch,
                ArrayIndex = arraySlice,
                MipLevel = mipIndex,
                CompleteCallback = completeCallback,
            });
        }

        public void SetData<T>(GraphicsPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arrayIndex, Action<GraphicsResource> completeCallback = null) 
            where T : unmanaged
        {
            // Store pending change.
            QueueTask(priority, new TextureSet<T>(data, startIndex, count)
            {
                Pitch = pitch,
                ArrayIndex = arrayIndex,
                MipLevel = level,
                CompleteCallback = completeCallback
            });
        }

        public void SetData<T>(GraphicsPriority priority, uint level, T* data, uint startIndex, uint count, uint pitch, uint arrayIndex, Action<GraphicsResource> completeCallback = null)
            where T : unmanaged
        {
            // Store pending change.
            QueueTask(priority, new TextureSet<T>(data, startIndex, count)
            {
                Pitch = pitch,
                ArrayIndex = arrayIndex,
                MipLevel = level,
                CompleteCallback = completeCallback
            });
        }

        public void GetData(GraphicsPriority priority, ITexture stagingTexture, Action<TextureData> callback)
        {
            QueueTask(priority, new TextureGetTask()
            {
                Staging = stagingTexture as TextureDX11,
                CompleteCallback = callback,
            });
        }

        public void GetData(GraphicsPriority priority, ITexture stagingTexture, uint mipLevel, uint arrayIndex, Action<TextureSlice> callback)
        {
            QueueTask(priority, new TextureGetSliceTask()
            {
                StagingTexture = stagingTexture as TextureDX11,
                CompleteCallback = callback,
                ArrayIndex = arrayIndex,
                MipMapLevel = mipLevel,
            });
        }

        /// <summary>A private helper method for retrieving the data of a subresource.</summary>
        /// <param name="cmd">The command queue that is to perform the retrieval.</param>
        /// <param name="staging">The staging texture to copy the data to.</param>
        /// <param name="level">The mip-map level.</param>
        /// <param name="arraySlice">The array slice.</param>
        /// <returns></returns>
        internal unsafe TextureSlice OnGetSliceData(GraphicsCommandQueue cmd, TextureDX11 staging, uint level, uint arraySlice)
        {
            uint subID = (arraySlice * MipMapCount) + level;
            uint subWidth = Width >> (int)level;
            uint subHeight = Height >> (int)level;

            ID3D11Resource* resToMap = _native;
            CommandQueueDX11 cmdNative = cmd as CommandQueueDX11;

            if (staging != null)
            {
                cmdNative.CopyResourceRegion(_native, subID, null, staging._native, subID, Vector3UI.Zero);
                cmd.Profiler.Current.CopySubresourceCount++;
                resToMap = staging._native;
            }

            // Now pull data from it
            MappedSubresource mapping = cmdNative.MapResource(resToMap, subID, Map.Read, 0);
            // NOTE: Databox: "The row pitch in the mapping indicate the offsets you need to use to jump between rows."
            // https://gamedev.stackexchange.com/questions/106308/problem-with-id3d11devicecontextcopyresource-method-how-to-properly-read-a-t/106347#106347


            uint blockSize = BCHelper.GetBlockSize(DataFormat);
            uint expectedRowPitch = 4 * Width; // 4-bytes per pixel * Width.
            uint expectedSlicePitch = expectedRowPitch * Height;

            if (blockSize > 0)
                BCHelper.GetBCLevelSizeAndPitch(subWidth, subHeight, blockSize, out expectedSlicePitch, out expectedRowPitch);

            byte[] sliceData = new byte[expectedSlicePitch];
            fixed (byte* ptrFixedSlice = sliceData)
            {
                byte* ptrSlice = ptrFixedSlice;
                byte* ptrDatabox = (byte*)mapping.PData;

                uint p = 0;
                while (p < mapping.DepthPitch)
                {
                    Buffer.MemoryCopy(ptrDatabox, ptrSlice, expectedSlicePitch, expectedRowPitch);
                    ptrDatabox += mapping.RowPitch;
                    ptrSlice += expectedRowPitch;
                    p += mapping.RowPitch;
                }
            }
            cmdNative.UnmapResource(_native, subID);

            TextureSlice slice = new TextureSlice(subWidth, subHeight, sliceData)
            {
                Pitch = expectedRowPitch,
            };

            return slice;
        }

        internal void OnSetSize(ref TextureResizeTask task)
        {
            // Avoid resizing/recreation if nothing has actually changed.
            if (Width == task.NewWidth && 
                Height == task.NewHeight && 
                Depth == task.NewDepth && 
                MipMapCount == task.NewMipMapCount && 
                ArraySize == task.NewArraySize && 
                DataFormat == task.NewFormat)
                return;

            Width = Math.Max(1, task.NewWidth);
            Height = Math.Max(1, task.NewHeight);
            Depth = Math.Max(1, task.NewDepth);
            MipMapCount = Math.Max(1, task.NewMipMapCount);
            DxgiFormat = task.NewFormat.ToApi();

            UpdateDescription(Width, Height, Depth, Math.Max(1, task.NewMipMapCount), Math.Max(1, task.NewArraySize), DxgiFormat);
            CreateTexture(true);
            OnResize?.Invoke(this);
        }


        protected virtual void UpdateDescription(uint newWidth, uint newHeight, 
            uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat) { }

        protected abstract ID3D11Resource* CreateResource(bool resize);

        public void Resize(GraphicsPriority priority, uint newWidth)
        {
            Resize(priority, newWidth, MipMapCount, DataFormat);
        }

        public void Resize(GraphicsPriority priority, uint newWidth, uint newMipMapCount, GraphicsFormat newFormat)
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

        public void CopyTo(GraphicsPriority priority, ITexture destination, Action<GraphicsResource> completeCallback = null)
        {
            TextureDX11 destTexture = destination as TextureDX11;

            if (DataFormat != destination.DataFormat)
                throw new TextureCopyException(this, destTexture, "The source and destination texture formats do not match.");

            if (destination.HasFlags(TextureFlags.Dynamic))
                throw new TextureCopyException(this, destination as TextureDX11, "Cannot copy to a dynamic texture via GPU. GPU cannot write to dynamic textures.");

            // Validate dimensions.
            if (destTexture.Width != Width ||
                destTexture.Height != Height ||
                destTexture.Depth != Depth)
                throw new TextureCopyException(this, destTexture, "The source and destination textures must have the same dimensions.");

            QueueTask(priority, new ResourceCopyTask()
            {
                Destination = destination as TextureDX11,
                CompletionCallback = completeCallback,
            });
        }

        public void CopyTo(GraphicsPriority priority, 
            uint sourceLevel, uint sourceSlice, 
            ITexture destination, uint destLevel, uint destSlice, 
            Action<GraphicsResource> completeCallback = null)
        {
            TextureDX11 destTexture = destination as TextureDX11;

            if (destination.HasFlags(TextureFlags.Dynamic))
                throw new TextureCopyException(this, destTexture, "Cannot copy to a dynamic texture via GPU. GPU cannot write to dynamic textures.");

            if (DataFormat != destination.DataFormat)
                throw new TextureCopyException(this, destTexture, "The source and destination texture formats do not match.");

            // Validate dimensions.
            // TODO this should only test the source and destination level dimensions, not the textures themselves.
            if (destTexture.Width != Width ||
                destTexture.Height != Height ||
                destTexture.Depth != Depth)
                throw new TextureCopyException(this, destTexture, "The source and destination textures must have the same dimensions.");

            if (sourceLevel >= MipMapCount)
                throw new TextureCopyException(this, destTexture, "The source mip-map level exceeds the total number of levels in the source texture.");

            if (sourceSlice >= ArraySize)
                throw new TextureCopyException(this, destTexture, "The source array slice exceeds the total number of slices in the source texture.");

            if (destLevel >= destTexture.MipMapCount)
                throw new TextureCopyException(this, destTexture, "The destination mip-map level exceeds the total number of levels in the destination texture.");

            if (destSlice >= destTexture.ArraySize)
                throw new TextureCopyException(this, destTexture, "The destination array slice exceeds the total number of slices in the destination texture.");

            QueueTask(priority, new SubResourceCopyTask()
            {
                SrcRegion = null,
                SrcSubResource = (sourceSlice * MipMapCount) + sourceLevel,
                DestResource = destination as ResourceDX11,
                DestStart = Vector3UI.Zero,
                DestSubResource = (destSlice * destination.MipMapCount) + destLevel,
                CompletionCallback = completeCallback,
            });
        }

        /// <summary>Applies all pending changes to the texture. Take care when calling this method in multi-threaded code. Calling while the
        /// GPU may be using the texture will cause unexpected behaviour.</summary>
        /// <param name="cmd"></param>
        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (IsDisposed)
                return;

            if(_native == null)
                CreateTexture(false);

            base.OnApply(cmd);
        }

        /// <summary>Gets the flags that were passed in when the texture was created.</summary>
        public TextureFlags AccessFlags { get; protected set; }

        public override GraphicsResourceFlags Flags { get; }

        /// <summary>Gets the format of the texture.</summary>
        public Format DxgiFormat { get; protected set; }

        public GraphicsFormat DataFormat => (GraphicsFormat)DxgiFormat;

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        public bool IsBlockCompressed { get; protected set; }

        /// <summary>Gets the width of the texture.</summary>
        public uint Width { get; protected set; }

        /// <summary>Gets the height of the texture.</summary>
        public uint Height { get; protected set; }

        /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
        public uint Depth { get; protected set; }

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public uint MipMapCount { get; protected set; }

        /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
        public uint ArraySize { get; protected set; }

        public override uint SizeInBytes { get; }

        /// <summary>
        /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
        /// </summary>
        public AntiAliasLevel MultiSampleLevel { get; protected set; }

        public MSAAQuality SampleQuality { get; protected set; }

        /// <summary>
        /// Gets whether or not the texture is multisampled. This is true if <see cref="MultiSamplingLevel"/> is greater than 1.
        /// </summary>
        public bool IsMultisampled => MultiSampleLevel >= AntiAliasLevel.X2;

        public bool IsValid { get; protected set; }

        internal override unsafe ID3D11Resource* ResourcePtr => _native;

        /// <summary>
        /// Gets the renderer that the texture is bound to.
        /// </summary>
        public RenderService Renderer { get; }
    }
}
