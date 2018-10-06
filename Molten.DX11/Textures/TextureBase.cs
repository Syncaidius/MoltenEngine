﻿using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.DXGI;
using Molten.Collections;
using Molten.Graphics.Textures.DDS;
using System.Runtime.InteropServices;
using System.Threading;
using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    using Resource = SharpDX.Direct3D11.Resource;

    public delegate void TextureEvent(TextureBase texture);

    public abstract partial class TextureBase : PipelineShaderObject
    {
        protected ShaderResourceViewDescription _resourceViewDescription;
        ThreadedQueue<ITextureChange> _pendingChanges;

        /// <summary>Triggered right before the internal texture resource is created.</summary>
        public event TextureEvent OnPreCreate;

        /// <summary>Triggered after the internal texture resource has been created.</summary>
        public event TextureEvent OnCreate;

        /// <summary>Triggered if the creation of the internal texture resource has failed (resulted in a null resource).</summary>
        public event TextureEvent OnCreateFailed;

        protected TextureFlags _flags;
        protected bool _isBlockCompressed;
        protected Format _format;

        protected int _width;
        protected int _height;
        protected int _depth;
        protected int _mipCount;
        protected int _arraySize;
        protected int _sampleCount;
        protected Resource _resource;
        RendererDX11 _renderer;

        static int _nextSortKey = 0;

        internal TextureBase(RendererDX11 renderer, int width, int height, int depth, int mipCount, int arraySize, int sampleCount, Format format, TextureFlags flags) : base(renderer.Device)
        {
            SortKey = Interlocked.Increment(ref _nextSortKey);
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

            _resourceViewDescription = new ShaderResourceViewDescription();
            _isBlockCompressed = DXTHelper.GetBlockCompressed(_format.FromApi());
        }

        private void ValidateFlagCombination()
        {
            // Validate RT mip-maps
            if (HasFlags(TextureFlags.AllowMipMapGeneration))
            {
                if(HasFlags(TextureFlags.NoShaderResource) || !(this is RenderSurfaceBase))
                    throw new TextureFlagException(_flags, "Mip-map generation is only available on render-surface shader resources.");
            }

            if (HasFlags(TextureFlags.Staging))
            {
                if (_flags != (TextureFlags.Staging) && _flags != (TextureFlags.Staging | TextureFlags.NoShaderResource))
                    throw new TextureFlagException(_flags, "Staging textures cannot have other flags set except NoShaderResource.");

                _flags |= TextureFlags.NoShaderResource;
            }
        }

        protected BindFlags GetBindFlags()
        {
            BindFlags result = BindFlags.None;

            if (HasFlags(TextureFlags.AllowUAV))
                result |= BindFlags.UnorderedAccess;

            if (!HasFlags(TextureFlags.NoShaderResource))
                result |= BindFlags.ShaderResource;

            if (this is RenderSurfaceBase)
                result |= BindFlags.RenderTarget;

            if (this is DepthSurface)
                result |= BindFlags.DepthStencil;

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

        protected CpuAccessFlags GetAccessFlags()
        {
            if (HasFlags(TextureFlags.Staging))
                return CpuAccessFlags.Read;
            else if (HasFlags(TextureFlags.Dynamic))
                return CpuAccessFlags.Write;
            else
                return CpuAccessFlags.None;

        }

        /// <summary>Gets the total byte size of a slice based on the format of the texture.</summary>
        /// <param name="sliceWidth">The slice width.</param>
        /// <param name="sliceHeight">The slice height.</param>
        /// <returns></returns>
        private int GetUncompressedByteSize(int sliceWidth, int sliceHeight, int sliceDepth)
        {
            int pixels = (sliceWidth * sliceHeight * sliceDepth);

            GraphicsFormat format = _format.FromApi();
            switch (format) {
                default:
                case GraphicsFormat.Unknown:
                    return pixels * 4;

                case GraphicsFormat.R1_UNorm:
                    return Math.Min(1, pixels / 8);

                case GraphicsFormat.A8_UNorm:
                case GraphicsFormat.R8_SInt:
                case GraphicsFormat.R8_SNorm:
                case GraphicsFormat.R8_Typeless:
                case GraphicsFormat.R8_UInt:
                case GraphicsFormat.R8_UNorm:
                    return pixels; // 1 byte

                case GraphicsFormat.B5G6R5_UNorm:
                case GraphicsFormat.B5G5R5A1_UNorm:
                case GraphicsFormat.D16_UNorm:
                case GraphicsFormat.R16_Float:
                case GraphicsFormat.R16_SInt:
                case GraphicsFormat.R16_SNorm:
                case GraphicsFormat.R16_Typeless:
                case GraphicsFormat.R16_UInt:
                case GraphicsFormat.R16_UNorm:
                case GraphicsFormat.R8G8_SInt:
                case GraphicsFormat.R8G8_SNorm:
                case GraphicsFormat.R8G8_Typeless:
                case GraphicsFormat.R8G8_UInt:
                case GraphicsFormat.R8G8_UNorm:
                    return pixels * 2; // 2 bytes             

                case GraphicsFormat.B8G8R8A8_Typeless:
                case GraphicsFormat.B8G8R8A8_UNorm:
                case GraphicsFormat.B8G8R8A8_UNorm_SRgb:
                case GraphicsFormat.B8G8R8X8_Typeless:
                case GraphicsFormat.B8G8R8X8_UNorm:
                case GraphicsFormat.B8G8R8X8_UNorm_SRgb:
                case GraphicsFormat.D32_Float:
                case GraphicsFormat.D24_UNorm_S8_UInt:
                case GraphicsFormat.G8R8_G8B8_UNorm:
                case GraphicsFormat.R10G10B10A2_Typeless:
                case GraphicsFormat.R10G10B10A2_UInt:
                case GraphicsFormat.R10G10B10A2_UNorm:
                case GraphicsFormat.R10G10B10_Xr_Bias_A2_UNorm:
                case GraphicsFormat.R11G11B10_Float:
                case GraphicsFormat.R16G16_Float:
                case GraphicsFormat.R16G16_SInt:
                case GraphicsFormat.R16G16_SNorm:
                case GraphicsFormat.R16G16_Typeless:
                case GraphicsFormat.R16G16_UInt:
                case GraphicsFormat.R16G16_UNorm:
                case GraphicsFormat.R24G8_Typeless:
                case GraphicsFormat.R24_UNorm_X8_Typeless:
                case GraphicsFormat.R32_Float:
                case GraphicsFormat.R32_SInt:
                case GraphicsFormat.R32_Typeless:
                case GraphicsFormat.R32_UInt:
                case GraphicsFormat.R8G8B8A8_SInt:
                case GraphicsFormat.R8G8B8A8_SNorm:
                case GraphicsFormat.R8G8B8A8_Typeless:
                case GraphicsFormat.R8G8B8A8_UInt:
                case GraphicsFormat.R8G8B8A8_UNorm:
                case GraphicsFormat.R8G8B8A8_UNorm_SRgb:
                case GraphicsFormat.R8G8_B8G8_UNorm:
                case GraphicsFormat.R9G9B9E5_Sharedexp:
                case GraphicsFormat.X24_Typeless_G8_UInt:
                    return pixels * 4; // 4 bytes

                case GraphicsFormat.D32_Float_S8X24_UInt:
                case GraphicsFormat.R16G16B16A16_Float:
                case GraphicsFormat.R16G16B16A16_SInt:
                case GraphicsFormat.R16G16B16A16_SNorm:
                case GraphicsFormat.R16G16B16A16_Typeless:
                case GraphicsFormat.R16G16B16A16_UInt:
                case GraphicsFormat.R16G16B16A16_UNorm:
                case GraphicsFormat.R32G32_Float:
                case GraphicsFormat.R32G32_SInt:
                case GraphicsFormat.R32G32_Typeless:
                case GraphicsFormat.R32G32_UInt:
                case GraphicsFormat.R32G8X24_Typeless:
                case GraphicsFormat.R32_Float_X8X24_Typeless:
                case GraphicsFormat.X32_Typeless_G8X24_UInt:
                    return pixels * 8;

                case GraphicsFormat.R32G32B32_Float:
                case GraphicsFormat.R32G32B32_SInt:
                case GraphicsFormat.R32G32B32_Typeless:
                case GraphicsFormat.R32G32B32_UInt:
                    return pixels * 12;

                case GraphicsFormat.R32G32B32A32_Float:
                case GraphicsFormat.R32G32B32A32_SInt:
                case GraphicsFormat.R32G32B32A32_Typeless:
                case GraphicsFormat.R32G32B32A32_UInt:
                    return pixels * 16;
            }
        }

        private void CreateUAV()
        {
            //check if UAVs are allowed.
            if (HasFlags(TextureFlags.AllowUAV) == false)
                return;

            //dispose of the old UAV, if it exists.
            if (UAV != null)
                UAV.Dispose();

            OnCreateUAV();
        }

        protected void CreateTexture(bool resize)
        {
            OnPreCreate?.Invoke(this);

            // Dispose of old resources
            OnDisposeForRecreation();
            _resource = CreateTextureInternal(resize);

            if (_resource != null)
            {
                //TrackAllocation();

                if (!HasFlags(TextureFlags.NoShaderResource))
                    CreateSRV();

                if (HasFlags(TextureFlags.AllowUAV))
                    CreateUAV();

                OnCreate?.Invoke(this);
            }
            else
            {
                OnCreateFailed?.Invoke(this);
            }

            IsValid = _resource != null;
        }

        /// <summary>Attempts to create a shader resource view (SRV) for the texture.</summary>
        protected virtual void CreateSRV()
        {
            // Dispose of old SRV.
            SRV?.Dispose();
            SRV = new ShaderResourceView(Device.D3d, _resource, _resourceViewDescription);
        }

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

        /// <summary>Generates mip maps for the texture via the provided <see cref="GraphicsPipe"/>.</summary>
        public void GenerateMipMaps()
        {
            if (!((_flags & TextureFlags.AllowMipMapGeneration) == TextureFlags.AllowMipMapGeneration))
                throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

            TexturegenMipMaps change = new TexturegenMipMaps();
            _pendingChanges.Enqueue(change);
        }

        internal void GenerateMipMaps(GraphicsPipe pipe)
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

        /// <summary>A private helper method for retrieving the data of a subresource.</summary>
        /// <param name="pipe">The pipe to perform the retrieval.</param>
        /// <param name="stagingTexture">The staging texture to copy the data to.</param>
        /// <param name="level">The mip-map level.</param>
        /// <param name="arraySlice">The array slice.</param>
        /// <param name="copySubresource">Copies the data via the provided staging texture. If this is true, the staging texture cannot be null.</param>
        /// <returns></returns>
        internal TextureData.Slice GetSliceData(GraphicsPipe pipe, TextureBase stagingTexture, int level, int arraySlice)
        {
            TextureData.Slice result = null;

            int subID = (arraySlice * MipMapCount) + level;

            int subWidth = _width >> level;
            int subHeight = _height >> level;

            Resource mappedResource = _resource;

            if (stagingTexture != null)
            {
                pipe.Context.CopySubresourceRegion(_resource, subID, null, stagingTexture._resource, subID);
                pipe.Profiler.Current.CopySubresourceCount++;
                mappedResource = stagingTexture._resource;
            }

            // Now pull data from it
            DataStream mappedData;
            DataBox databox = pipe.Context.MapSubresource(
                mappedResource,
                subID,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out mappedData);
            {
                result = new TextureData.Slice()
                {
                    Width = subWidth,
                    Height = subHeight,
                    Data = mappedData.ReadRange<byte>(databox.SlicePitch),
                    Pitch = databox.RowPitch,
                    TotalBytes = databox.SlicePitch,
                };
            };

            pipe.Context.UnmapSubresource(mappedResource, 0);

            return result;
        }

        internal void SetSizeInternal(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            // Avoid resizing/recreation if nothing has actually changed.
            if (_width == newWidth && 
                _height == newHeight && 
                _depth == newDepth && 
                _mipCount == newMipMapCount && 
                _arraySize == newArraySize && 
                _format == newFormat)
                return;

            BeforeResize();
            _width = Math.Max(1, newWidth);
            _height = Math.Max(1, newHeight);
            _depth = Math.Max(1, newDepth);
            _mipCount = Math.Max(1, newMipMapCount);
            _format = newFormat;

            OnSetSize(_width, _height, _depth, Math.Max(1, newMipMapCount), Math.Max(1, newArraySize), newFormat);
            CreateTexture(true);
            AfterResize();
        }

        protected virtual void BeforeResize() { }

        protected virtual void AfterResize() { }

        protected virtual void OnSetSize(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat) { }

        protected abstract Resource CreateTextureInternal(bool isResizing);

        private protected void QueueChange(ITextureChange change)
        {
            _pendingChanges.Enqueue(change);
        }


        public void Resize(int newWidth)
        {
            Resize(newWidth, _mipCount, _format.FromApi());
        }

        public void Resize(int newWidth, int newMipMapCount, GraphicsFormat newFormat)
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

            if (this.Format != destination.Format)
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

            if (this.Format != destination.Format)
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
        internal void Apply(GraphicsPipe pipe)
        {
            if (IsDisposed)
                return;

            if (_resource == null)
                CreateTexture(false);

            // process all changes for the current pipe.
            while (_pendingChanges.Count > 0)
            {
                ITextureChange change = null;
                if (_pendingChanges.TryDequeue(out change))
                    change.Process(pipe, this);
            }
        }

        internal override void Refresh(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            Apply(pipe);
        }

        protected virtual void OnCreateUAV() { }

        /// <summary>Gets the flags that were passed in when the texture was created.</summary>
        public TextureFlags Flags => _flags;

        /// <summary>Gets the format of the texture.</summary>
        public Format DxFormat => _format;

        public GraphicsFormat Format => (GraphicsFormat)_format;

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        public bool IsBlockCompressed => _isBlockCompressed;

        /// <summary>Gets the width of the texture.</summary>
        public int Width => _width;

        /// <summary>Gets the height of the texture.</summary>
        public int Height => _height;

        /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
        public int Depth => _depth;

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public int MipMapCount => _mipCount;

        /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
        public int ArraySize => _arraySize;

        /// <summary>Gets whether or not the texture is a texture array.</summary>
        public bool IsTextureArray => _arraySize > 1;

        /// <summary>
        /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
        /// </summary>
        public int SampleCount => _sampleCount;

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
        public MoltenRenderer Renderer => _renderer;

        /// <summary>
        /// Gets the sort key associated with the current texture.
        /// </summary>
        public int SortKey { get; }
    }
}