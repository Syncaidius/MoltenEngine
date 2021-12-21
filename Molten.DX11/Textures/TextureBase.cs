using System;
using System.Collections.Generic;
using System.Linq;
using Molten.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using Molten.Graphics.Textures;
using Silk.NET.DXGI;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public delegate void TextureEvent(TextureBase texture);

    public abstract partial class TextureBase : PipeBindableResource, ITexture
    {
        ThreadedQueue<ITextureChange> _pendingChanges;

        /// <summary>Triggered right before the internal texture resource is created.</summary>
        public event TextureEvent OnPreCreate;

        /// <summary>Triggered after the internal texture resource has been created.</summary>
        public event TextureEvent OnCreate;

        /// <summary>Triggered if the creation of the internal texture resource has failed (resulted in a null resource).</summary>
        public event TextureEvent OnCreateFailed;

        /// <summary>
        /// Invokved right before resizing of the texture begins.
        /// </summary>
        public event TextureHandler OnPreResize;

        /// <summary>
        /// Invoked after resizing of the texture has completed.
        /// </summary>
        public event TextureHandler OnPostResize;

        protected TextureFlags _flags;
        protected bool _isBlockCompressed;
        protected Format _format;

        protected uint _width;
        protected uint _height;
        protected uint _depth;
        protected uint _mipCount;
        protected uint _arraySize;
        protected uint _sampleCount;
        protected Resource _resource;
        RendererDX11 _renderer;

        ShaderResourceViewDescription _srvDescription;
        UnorderedAccessViewDescription _uavDescription;

        static int _nextSortKey = 0;

        internal TextureBase(RendererDX11 renderer, int width, int height, int depth, int mipCount, 
            int arraySize, int sampleCount, Format format, TextureFlags flags) : base(renderer.Device)
        {
            _renderer = renderer;
            _flags = flags;
            ValidateFlagCombination();

            _pendingChanges = new ThreadedQueue<ITextureChange>();

            _width = width;
            _height = height;
            _depth = depth;
            _mipCount = mipCount;
            _arraySize = arraySize;
            _sampleCount = sampleCount;
            _format = format;
            IsValid = false;

            _srvDescription = new ShaderResourceViewDescription();
            _isBlockCompressed = BCHelper.GetBlockCompressed(_format.FromApi());
        }

        public Texture1DProperties Get1DProperties()
        {
            return new Texture1DProperties()
            {
                Width = _width,
                ArraySize = _arraySize,
                Flags = _flags,
                Format = this.DataFormat,
                MipMapLevels = _mipCount,
            };
        }

        protected void RaisePostResizeEvent()
        {
            OnPostResize?.Invoke(this);
        }

        private void ValidateFlagCombination()
        {
            // Validate RT mip-maps
            if (HasFlags(TextureFlags.AllowMipMapGeneration))
            {
                if(HasFlags(TextureFlags.NoShaderResource) || !(this is RenderSurface))
                    throw new TextureFlagException(_flags, "Mip-map generation is only available on render-surface shader resources.");
            }

            if (HasFlags(TextureFlags.Staging))
            {
                if (_flags != (TextureFlags.Staging) && _flags != (TextureFlags.Staging | TextureFlags.NoShaderResource))
                    throw new TextureFlagException(_flags, "Staging textures cannot have other flags set except NoShaderResource.");

                _flags |= TextureFlags.NoShaderResource;
            }
        }

        protected BindFlag GetBindFlags()
        {
            BindFlag result = 0;

            if (HasFlags(TextureFlags.AllowUAV))
                result |= BindFlag.BindUnorderedAccess;

            if (!HasFlags(TextureFlags.NoShaderResource))
                result |= BindFlag.BindShaderResource;

            if (this is RenderSurface)
                result |= BindFlag.BindRenderTarget;

            if (this is DepthStencilSurface)
                result |= BindFlag.BindDepthStencil;

            return result;
        }

        protected ResourceOptionFlags GetResourceFlags()
        {
            ResourceOptionFlags result = ResourceOptionFlags.None;

            if (HasFlags(TextureFlags.SharedResource))
                result |= ResourceOptionFlags.Shared;

            if (HasFlags(TextureFlags.AllowMipMapGeneration))
                result |= ResourceOptionFlags.GenerateMipMaps;

            return result;
        }

        protected ResourceUsage GetUsageFlags()
        {
            if (HasFlags(TextureFlags.Staging))
                return ResourceUsage.Staging;
            else if (HasFlags(TextureFlags.Dynamic))
                return ResourceUsage.Dynamic;
            else
                return ResourceUsage.Default;
        }

        protected CpuAccessFlag GetAccessFlags()
        {
            if (HasFlags(TextureFlags.Staging))
                return CpuAccessFlag.CpuAccessRead;
            else if (HasFlags(TextureFlags.Dynamic))
                return CpuAccessFlag.CpuAccessWrite;
            else
                return 0;
        }

        protected void CreateTexture(bool resize)
        {
            OnPreCreate?.Invoke(this);

            // Dispose of old resources
            OnDisposeForRecreation();
            _resource = CreateResource(resize);

            if (_resource != null)
            {
                UAV?.Dispose();
                SRV?.Dispose();

                //TrackAllocation();

                if (!HasFlags(TextureFlags.NoShaderResource))
                {
                    SetSRVDescription(ref _srvDescription);
                    SRV = new ShaderResourceView(Device.D3d, _resource, _srvDescription);
                }

                if (HasFlags(TextureFlags.AllowUAV))
                {
                    SetUAVDescription(_srvDescription, ref _uavDescription);
                    UAV = new UnorderedAccessView(Device.D3d, _resource, _uavDescription);
                }

                OnCreate?.Invoke(this);
            }
            else
            {
                OnCreateFailed?.Invoke(this);
            }

            IsValid = _resource != null;
        }

        protected abstract void SetUAVDescription(ShaderResourceViewDescription srvDesc, ref UnorderedAccessViewDescription desc);

        protected abstract void SetSRVDescription(ref ShaderResourceViewDescription desc);

        protected virtual void OnDisposeForRecreation()
        {
            OnPipelineDispose();
        }

        private protected override void OnPipelineDispose()
        {
            base.OnPipelineDispose();

            //TrackDeallocation();
            DisposeObject(ref _resource);
        }

        public bool HasFlags(TextureFlags flags)
        {
            return (_flags & flags) == flags;
        }

        /// <summary>Queries the underlying texture's interface.</summary>
        /// <typeparam name="T">The type of object to request in the query.</typeparam>
        /// <returns></returns>
        public T Query<T>() where T : ComObject
        {
            if (_resource != null)
                return _resource.QueryInterface<T>();

            return null;
        }

        /// <summary>Generates mip maps for the texture via the provided <see cref="PipeDX11"/>.</summary>
        public void GenerateMipMaps()
        {
            if (!((_flags & TextureFlags.AllowMipMapGeneration) == TextureFlags.AllowMipMapGeneration))
                throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

            TexturegenMipMaps change = new TexturegenMipMaps();
            _pendingChanges.Enqueue(change);
        }

        internal void GenerateMipMaps(PipeDX11 pipe)
        {
            if (SRV != null)
                pipe.Context.GenerateMips(SRV);
        }

        public void SetData<T>(Rectangle area, T[] data, int bytesPerPixel, int level, int arrayIndex = 0) where T : struct
        {
            int eSize = Marshal.SizeOf(typeof(T));
            int count = data.Length;
            int texturePitch = area.Width * bytesPerPixel;
            int pixels = area.Width * area.Height;

            int expectedBytes = pixels * bytesPerPixel;
            int dataBytes = data.Length * eSize;

            if (pixels != data.Length)
                throw new Exception($"The provided data does not match the provided area of {area.Width}x{area.Height}. Expected {expectedBytes} bytes. {dataBytes} bytes were provided.");

            // Do a bounds check
            Rectangle texBounds = new Rectangle(0, 0, _width, _height);
            if (!texBounds.Contains(area))
                throw new Exception("The provided area would go outside of the current texture's bounds.");

            TextureSet<T> change = new TextureSet<T>()
            {
                Stride = eSize,
                Count = count,
                Data = new T[count],
                Pitch = texturePitch,
                StartIndex = 0,
                ArrayIndex = arrayIndex,
                MipLevel = level,
                Area = area,
            };

            //copy the data so that it is not affected by other threads
            Array.Copy(data, 0, change.Data, 0, count);

            _pendingChanges.Enqueue(change);
        }

        /// <summary>Copies data fom the provided <see cref="TextureData"/> instance into the current texture.</summary>
        /// <param name="data"></param>
        /// <param name="srcMipIndex">The starting mip-map index within the provided <see cref="TextureData"/>.</param>
        /// <param name="srcArraySlice">The starting array slice index within the provided <see cref="TextureData"/>.</param>
        /// <param name="mipCount">The number of mip-map levels to copy per array slice, from the provided <see cref="TextureData"/>.</param>
        /// <param name="arrayCount">The number of array slices to copy from the provided <see cref="TextureData"/>.</param>
        /// <param name="destMipIndex">The mip-map index within the current texture to start copying to.</param>
        /// <param name="destArraySlice">The array slice index within the current texture to start copying to.<</param>
        public void SetData(TextureData data, int srcMipIndex, int srcArraySlice, int mipCount, int arrayCount, int destMipIndex = 0, int destArraySlice = 0)
        {
            TextureData.Slice level = null;

            for(int a = 0; a < arrayCount; a++)
            {
                for(int m = 0; m < mipCount; m++)
                {
                    int slice = srcArraySlice + a;
                    int mip = srcMipIndex + m;
                    int dataID = TextureData.GetLevelID(data.MipMapLevels, mip, slice);
                    level = data.Levels[dataID];

                    if (level.TotalBytes == 0)
                        continue;

                    int destSlice = destArraySlice + a;
                    int destMip = destMipIndex + m;
                    SetData(destMip, level.Data, 0, level.TotalBytes, level.Pitch, destSlice);
                }
            }
        }

        public void SetData(TextureData.Slice data, int mipIndex, int arraySlice)
        {
            TextureSet<byte> change = new TextureSet<byte>()
            {
                Stride = 1,
                Count = data.TotalBytes,
                Data = data.Data.Clone() as byte[],
                Pitch = data.Pitch,
                StartIndex = 0,
                ArrayIndex = arraySlice,
                MipLevel = mipIndex,
            };

            // Store pending change.
            _pendingChanges.Enqueue(change);
        }

        public void SetData<T>(int level, T[] data, int startIndex, int count, int pitch, int arrayIndex) where T : struct
        {
            int eSize = Marshal.SizeOf(typeof(T));

            TextureSet<T> change = new TextureSet<T>()
            {
                Stride = eSize,
                Count = count,
                Data = new T[count],
                Pitch = pitch,
                StartIndex = startIndex,
                ArrayIndex = arrayIndex,
                MipLevel = level,
            };

            //copy the data so that it is not affected by other threads
            Array.Copy(data, startIndex, change.Data, 0, count);

            // Store pending change.
            _pendingChanges.Enqueue(change);
        }

        public void GetData(ITexture stagingTexture, Action<TextureData> callback)
        {
            _pendingChanges.Enqueue(new TextureGet()
            {
                StagingTexture = stagingTexture as TextureBase,
                Callback = callback,
            });
        }

        public void GetData(ITexture stagingTexture, int mipLevel, int arrayIndex, Action<TextureData.Slice> callback)
        {
            _pendingChanges.Enqueue(new TextureGetSlice()
            {
                StagingTexture = stagingTexture as TextureBase,
                Callback = callback,
                ArrayIndex = arrayIndex,
                MipMapLevel = mipLevel,
            });
        }

        internal TextureData GetAllData(PipeDX11 pipe, TextureBase staging)
        {
            if (staging == null && !HasFlags(TextureFlags.Staging))
                throw new TextureCopyException(this, null, "A null staging texture was provided, but this is only valid if the current texture is a staging texture. A staging texture is required to retrieve data from non-staged textures.");

            if (!staging.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(staging.Flags, "Provided staging texture does not have the staging flag set.");

            // Validate dimensions.
            if (staging.Width != Width ||
                staging.Height != Height ||
                staging.Depth != Depth)
                throw new TextureCopyException(this, staging, "Staging texture dimensions do not match current texture.");

            staging.Apply(pipe);

            Resource resToMap = _resource;

            if (staging != null)
            {
                pipe.Context.CopyResource(_resource, staging.UnderlyingResource);
                pipe.Profiler.Current.CopyResourceCount++;
                resToMap = staging._resource;
            }

            TextureData data = new TextureData()
            {
                ArraySize = _arraySize,
                Flags = _flags,
                Format = DataFormat,
                Height = _height,
                HighestMipMap = 0,
                IsCompressed = _isBlockCompressed,
                Levels = new TextureData.Slice[_arraySize * MipMapCount],
                MipMapLevels = _mipCount,
                Width = _width,
            };

            int blockSize = BCHelper.GetBlockSize(DataFormat);
            int expectedRowPitch = 4 * Width; // 4-bytes per pixel * Width.
            int expectedSlicePitch = expectedRowPitch * Height;

            // Iterate over each array slice.
            for (int a = 0; a < _arraySize; a++)
            {
                // Iterate over all mip-map levels of the array slice.
                for (int i = 0; i < _mipCount; i++)
                {
                    int subID = (a * _mipCount) + i;
                    data.Levels[subID] = GetSliceData(pipe, staging, i, a);
                }
            }

            return data;
        }

        /// <summary>A private helper method for retrieving the data of a subresource.</summary>
        /// <param name="pipe">The pipe to perform the retrieval.</param>
        /// <param name="staging">The staging texture to copy the data to.</param>
        /// <param name="level">The mip-map level.</param>
        /// <param name="arraySlice">The array slice.</param>
        /// <param name="copySubresource">Copies the data via the provided staging texture. If this is true, the staging texture cannot be null.</param>
        /// <returns></returns>
        internal unsafe TextureData.Slice GetSliceData(PipeDX11 pipe, TextureBase staging, uint level, uint arraySlice)
        {
            uint subID = (arraySlice * MipMapCount) + level;
            uint subWidth = _width >> level;
            uint subHeight = _height >> level;

            Resource resToMap = _resource;

            if (staging != null)
            {
                pipe.Context.CopySubresourceRegion(_resource, subID, null, staging._resource, subID);
                pipe.Profiler.Current.CopySubresourceCount++;
                resToMap = staging._resource;
            }

            // Now pull data from it
            DataBox databox = pipe.Context.MapSubresource(resToMap, subID, Map.MapRead, MapFlags.None);
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
                byte* ptrDatabox = (byte*)databox.DataPointer.ToPointer();

                int p = 0;
                while (p < databox.SlicePitch)
                {
                    System.Buffer.MemoryCopy(ptrDatabox, ptrSlice, expectedSlicePitch, expectedRowPitch);
                    ptrDatabox += databox.RowPitch;
                    ptrSlice += expectedRowPitch;
                    p += databox.RowPitch;
                }
            }
            pipe.Context.UnmapSubresource(_resource, subID);

            TextureData.Slice slice = new TextureData.Slice()
            {
                Width = subWidth,
                Height = subHeight,
                Data = sliceData,
                Pitch = expectedRowPitch,
                TotalBytes = expectedSlicePitch,
            };

            return slice;
        }

        internal void SetSizeInternal(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            // Avoid resizing/recreation if nothing has actually changed.
            if (_width == newWidth && 
                _height == newHeight && 
                _depth == newDepth && 
                _mipCount == newMipMapCount && 
                _arraySize == newArraySize && 
                _format == newFormat)
                return;

            OnPreResize?.Invoke(this);
            _width = Math.Max(1, newWidth);
            _height = Math.Max(1, newHeight);
            _depth = Math.Max(1, newDepth);
            _mipCount = Math.Max(1, newMipMapCount);
            _format = newFormat;

            UpdateDescription(_width, _height, _depth, Math.Max(1, newMipMapCount), Math.Max(1, newArraySize), newFormat);
            CreateTexture(true);
            OnPostResize?.Invoke(this);
        }


        protected virtual void UpdateDescription(uint newWidth, uint newHeight, 
            uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat) { }

        protected abstract Resource CreateResource(bool resize);

        private protected void QueueChange(ITextureChange change)
        {
            _pendingChanges.Enqueue(change);
        }

        public void Resize(uint newWidth)
        {
            Resize(newWidth, _mipCount, _format.FromApi());
        }

        public void Resize(uint newWidth, uint newMipMapCount, GraphicsFormat newFormat)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = _height,
                NewMipMapCount = newMipMapCount,
                NewArraySize = _arraySize,
                NewFormat = _format,
            });
        }

        public void CopyTo(ITexture destination)
        {
            TextureBase destTexture = destination as TextureBase;

            if (this.DataFormat != destination.DataFormat)
                throw new TextureCopyException(this, destTexture, "The source and destination texture formats do not match.");

            // Validate dimensions.
            if (destTexture.Width != this.Width ||
                destTexture.Height != this.Height ||
                destTexture.Depth != this.Depth)
                throw new TextureCopyException(this, destTexture, "The source and destination textures must have the same dimensions.");

            QueueChange(new TextureCopyTo()
            {
                Destination = destination as TextureBase,
            });

            TextureApply applyTask = TextureApply.Get();
            applyTask.Texture = this;
            _renderer.PushTask(applyTask);
        }

        public void CopyTo(int sourceLevel, int sourceSlice, ITexture destination, int destLevel, int destSlice)
        {
            TextureBase destTexture = destination as TextureBase;

            if (destination.HasFlags(TextureFlags.Dynamic))
                throw new TextureCopyException(this, destTexture, "Cannot copy to a dynamic texture via GPU. GPU cannot write to dynamic textures.");

            if (this.DataFormat != destination.DataFormat)
                throw new TextureCopyException(this, destTexture, "The source and destination texture formats do not match.");

            // Validate dimensions.
            // TODO this should only test the source and destination level dimensions, not the textures themselves.
            if (destTexture.Width != this.Width ||
                destTexture.Height != this.Height ||
                destTexture.Depth != this.Depth)
                throw new TextureCopyException(this, destTexture, "The source and destination textures must have the same dimensions.");

            if (sourceLevel >= this.MipMapCount)
                throw new TextureCopyException(this, destTexture, "The source mip-map level exceeds the total number of levels in the source texture.");

            if (sourceSlice >= this.ArraySize)
                throw new TextureCopyException(this, destTexture, "The source array slice exceeds the total number of slices in the source texture.");

            if (destLevel >= destTexture.MipMapCount)
                throw new TextureCopyException(this, destTexture, "The destination mip-map level exceeds the total number of levels in the destination texture.");

            if (destSlice >= destTexture.ArraySize)
                throw new TextureCopyException(this, destTexture, "The destination array slice exceeds the total number of slices in the destination texture.");

            QueueChange(new TextureCopyLevel()
            {
                Destination = destination as TextureBase,
                SourceLevel = sourceLevel,
                SourceSlice = sourceSlice,
                DestinationLevel = destLevel,
                DestinationSlice = destSlice,
            });

            TextureApply applyTask = TextureApply.Get();
            applyTask.Texture = this;
            _renderer.PushTask(applyTask);
        }

        /// <summary>Applies all pending changes to the texture. Take care when calling this method in multi-threaded code. Calling while the
        /// GPU may be using the texture will cause unexpected behaviour.</summary>
        /// <param name="pipe"></param>
        internal void Apply(PipeDX11 pipe)
        {
            if (IsDisposed)
                return;

            if(_resource == null)
                CreateTexture(false);

            // process all changes for the current pipe.
            while (_pendingChanges.Count > 0)
            {
                ITextureChange change = null;
                if (_pendingChanges.TryDequeue(out change))
                    change.Process(pipe, this);
            }
        }

        internal override void Refresh(PipeDX11 pipe, PipelineBindSlot<DeviceDX11, PipeDX11> slot)
        {
            Apply(pipe);
        }

        /// <summary>Gets the flags that were passed in when the texture was created.</summary>
        public TextureFlags Flags => _flags;

        /// <summary>Gets the format of the texture.</summary>
        public Format DxFormat => _format;

        public GraphicsFormat DataFormat => (GraphicsFormat)_format;

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        public bool IsBlockCompressed => _isBlockCompressed;

        /// <summary>Gets the width of the texture.</summary>
        public uint Width => _width;

        /// <summary>Gets the height of the texture.</summary>
        public uint Height => _height;

        /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
        public uint Depth => _depth;

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public uint MipMapCount => _mipCount;

        /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
        public uint ArraySize => _arraySize;

        /// <summary>
        /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
        /// </summary>
        public uint SampleCount => _sampleCount;

        /// <summary>
        /// Gets whether or not the texture is multisampled. This is true if <see cref="SampleCount"/> is greater than 1.
        /// </summary>
        public bool IsMultisampled => _sampleCount > 1;

        public bool IsValid { get; protected set; }

        /// <summary>
        /// Gets the underlying texture resource.
        /// </summary>
        internal Resource UnderlyingResource => _resource;

        /// <summary>
        /// Gets the renderer that the texture is bound to.
        /// </summary>
        public RenderService Renderer => _renderer;
    }
}