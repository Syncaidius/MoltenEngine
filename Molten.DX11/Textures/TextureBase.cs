using SharpDX;
using SharpDX.Direct3D11;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.DXGI;
using Molten.Collections;
using Molten.Graphics.Textures.DDS;
using System.Runtime.InteropServices;

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

        protected long _curVramSize;
        protected Resource _resource;
        GraphicsDevice _device;

        protected ShaderResourceView _srv;
        protected UnorderedAccessView _uav;

        internal TextureBase(GraphicsDevice device, int width, int height, int depth, int mipCount = 1, 
            Format format = SharpDX.DXGI.Format.R8G8B8A8_UNorm, 
            TextureFlags flags = TextureFlags.None)
        {
            _flags = flags;
            _device = device;
            ValidateFlagCombination();

            _pendingChanges = new ThreadedQueue<ITextureChange>();

            _width = width;
            _height = height;
            _depth = depth;
            _mipCount = mipCount;

            _format = format;
            IsValid = false;

            _resourceViewDescription = new ShaderResourceViewDescription();
            _isBlockCompressed = GetBlockCompressed();
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

        private bool GetBlockCompressed()
        {
            GraphicsFormat format = _format.FromApi();

            //figure out if the texture is block compressed.
            switch (format)
            {
                case GraphicsFormat.BC1_UNorm:
                case GraphicsFormat.BC2_UNorm:
                case GraphicsFormat.BC3_UNorm:
                case GraphicsFormat.BC4_SNorm:
                case GraphicsFormat.BC4_UNorm:
                case GraphicsFormat.BC5_SNorm:
                case GraphicsFormat.BC5_UNorm:
                    return true;

                default:
                    return false;
            }
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
            DisposeObject(ref _srv);

            SRV = new ShaderResourceView(Device.D3d, _resource, _resourceViewDescription);
        }

        protected virtual void OnDisposeForRecreation()
        {
            OnDispose();
        }

        protected override void OnDispose()
        {
            //TrackDeallocation();

            DisposeObject(ref _srv);
            DisposeObject(ref _uav);
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
            if (_srv != null)
                pipe.Context.GenerateMips(_srv);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="srcMipIndex"></param>
        /// <param name="srcArraySlice"></param>
        /// <param name="mipCount"></param>
        /// <param name="arrayCount"></param>
        /// <param name="destMipIndex"></param>
        /// <param name="destArraySlice"></param>
        public void SetData(TextureData data, int srcMipIndex, int srcArraySlice, int mipCount, int arrayCount, int destMipIndex = 0, int destArraySlice = 0)
        {
            TextureData.Slice level = null;

            for(int a = 0; a < arrayCount; a++)
            {
                for(int m = 0; m < mipCount; m++)
                {
                    int slice = srcArraySlice + a;
                    int mip = srcMipIndex + m;
                    int dataID = TextureData.GetLevelID(data.MipMapCount, mip, slice);
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

        /// <summary>Writes texture data to a stream. NOTE: This is an extremely expensive operation, which is not thread-safe.</summary>
        /// <param name="mipLevel">The mip map level to write to the provided stream.</param>
        /// <param name="stagingTexture">A staging texture to use when retrieving data from the GPU. Only textures
        /// with the staging flag set will work.</param>
        internal TextureData GetData(GraphicsPipe pipe, TextureBase stagingTexture)
        {
            if (!stagingTexture.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(stagingTexture.Flags, "Provided staging texture does not have the staging flag set.");

            // Validate dimensions.
            if (stagingTexture._width != this._width ||
                stagingTexture._height != this._height ||
                stagingTexture._depth != this._depth)
                throw new TextureCopyException(this, stagingTexture, "Staging texture dimensions do not match current texture.");

            ApplyChanges(pipe);
            stagingTexture.ApplyChanges(pipe);

            // Copy the texture into the staging texture.
            Device.Context.CopyResource(_resource, stagingTexture._resource);

            TextureData data = new TextureData()
            {
                ArraySize = this.ArraySize,
                Flags = _flags,
                Format = _format.FromApi(),
                Height = _height,
                HighestMipMap = 0,
                IsCompressed = _isBlockCompressed,
                Levels = new TextureData.Slice[this.ArraySize * this.MipMapLevels],
                MipMapCount = this.MipMapLevels,
                Width = _width,
            };

            int levelID = 0;

            // Iterate over each array slice.
            for (int a = 0; a < this.ArraySize; a++)
            {
                // Iterate over all mip-map levels of the array slice.
                for (int i = 0; i < this.MipMapLevels; i++)
                {
                    levelID = (a * this.MipMapLevels) + i;
                    data.Levels[levelID] = GetSliceData(pipe, stagingTexture, i, a, false);
                }
            }

            // Return resulting data
            return data;
        }

        /// <summary>Returns the data from a single mip-map level within a slice of the texture. For 2D, non-array textures, this will always be slice 0.</summary>
        /// <param name="pipe">The graphics pipe to perform the retrieval.</param>
        /// <param name="stagingTexture">The staging texture to copy the data to, from the GPU.</param>
        /// <param name="level">The mip-map level to retrieve.</param>
        /// <param name="arraySlice">The array slice to access.</param>
        /// <returns></returns>
        internal TextureData.Slice GetData(GraphicsPipe pipe, TextureBase stagingTexture, int level, int arraySlice)
        {
            if (!stagingTexture.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(stagingTexture.Flags, "Provided staging texture does not have the staging flag set.");

            // Validate dimensions.
            if (stagingTexture._width != this._width ||
                stagingTexture._height != this._height ||
                stagingTexture._depth != this._depth)
                throw new TextureCopyException(this, stagingTexture, "Staging texture dimensions do not match current texture.");

            if (level >= MipMapLevels)
                throw new TextureCopyException(this, stagingTexture, "mip-map level must be less than the total mip-map levels of the texture.");

            if (arraySlice >= ArraySize)
                throw new TextureCopyException(this, stagingTexture, "array slice must be less than the array size of the texture.");

            ApplyChanges(pipe);
            stagingTexture.ApplyChanges(pipe);

            return GetSliceData(pipe, stagingTexture, level, arraySlice, true);
        }

        /// <summary>A private helper method for retrieving the data of a subresource.</summary>
        /// <param name="pipe">The pipe to perform the retrieval.</param>
        /// <param name="stagingTexture">The staging texture to copy the data to.</param>
        /// <param name="level">The mip-map level.</param>
        /// <param name="arraySlice">The array slice.</param>
        /// <param name="copySubresource">Copies the data via the provided staging texture. If this is true, the staging texture cannot be null.</param>
        /// <returns></returns>
        private TextureData.Slice GetSliceData(GraphicsPipe pipe, TextureBase stagingTexture, int level, int arraySlice, bool copySubresource)
        {
            TextureData.Slice result = null;

            int subID = (arraySlice * MipMapLevels) + level;

            int subWidth = _width >> level;
            int subHeight = _height >> level;

            if(copySubresource)
                pipe.Context.CopySubresourceRegion(_resource, subID, null, stagingTexture._resource, subID);

            // Now pull data from it
            DataStream mappedData;
            DataBox databox = pipe.Context.MapSubresource(
                stagingTexture._resource,
                subID,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out mappedData);
            {
                result= new TextureData.Slice()
                {
                    Width = subWidth,
                    Height = subHeight,
                    Data = mappedData.ReadRange<byte>(databox.SlicePitch),
                    Pitch = databox.RowPitch,
                    TotalBytes = databox.SlicePitch,
                };
            };

            pipe.Context.UnmapSubresource(stagingTexture._resource, 0);

            return result;
        }

        /// <summary></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level">The mip level to set the data.</param>
        /// <param name="data">The data.</param>
        /// <param name="startIndex">The byte offset in the data array to copy from.</param>
        /// <param name="elementCount">The number of data elements to copy.</param>
        /// <param name="pitch">The size of 1 row, in bytes.</param>
        /// <param name="arrayIndex">The index in the texture's array to set the data. 
        /// Ordinary textures generally have 1 array element, with an index of 0. Cube maps usually have an array size of 6, 
        /// where one element is 1 side of the cube map.</param>
        internal void SetDataInternal<T>(GraphicsPipe pipe, T[] data, int startIndex, int count, int stride, int mipLevel, int arrayIndex, int pitch, Rectangle? area = null) where T: struct
        {
            //calculate size of a single level
            int sliceBytes = 0;
            int blockSize = 8; // default block size
            int levelWidth = _width;
            int levelHeight = _height;

            if (_isBlockCompressed)
            {
                blockSize = DDSHelper.GetBlockSize(_format.FromApi());

                // Collect total level size.
                for (int i = 0; i < _mipCount; i++)
                {
                    sliceBytes += GetBCLevelSize(levelWidth, levelHeight, blockSize);
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }
            else
            {
                for (int i = 0; i < _mipCount; i++)
                {
                    sliceBytes += levelWidth * levelHeight * 4; //4 color channels. 1 byte each. Width * height * colorByteSize.
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }

            //======DATA TRANSFER===========
            EngineInterop.PinObject(data, (ptr) =>
            {
                int startBytes = startIndex * stride;
                IntPtr dataPtr = ptr + startBytes;
                int subLevel = (_mipCount * arrayIndex) + mipLevel;

                if (HasFlags(TextureFlags.Dynamic))
                {
                    DataStream stream = null;
                    DataBox destBox = pipe.Context.MapSubresource(_resource, subLevel, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);

                    // Are we constrained to an area of the texture?
                    if (area != null)
                    {
                        Rectangle rect = area.Value;
                        int areaPitch = stride * rect.Width;
                        int aX = rect.X;
                        int aY = rect.Y;
                        

                        for (int y = aY, end = rect.Bottom; y < end; y++)
                        {
                            stream.Position = (pitch * aY) + (aX * stride);
                            stream.WriteRange(dataPtr, areaPitch);

                            dataPtr += areaPitch;
                            aY++;
                        }
                    }
                    else
                    {
                        stream.WriteRange(dataPtr, count);
                    }

                    pipe.Context.UnmapSubresource(_resource, subLevel);
                }
                else
                {
                    if (_isBlockCompressed)
                    {
                        // Calculate mip-map level size.
                        levelWidth = _width >> mipLevel;
                        levelHeight = _height >> mipLevel;

                        int bcPitch = GetBCPitch(levelWidth, levelHeight, blockSize);
                        DataBox box = new DataBox(dataPtr, bcPitch, sliceBytes);

                        if (area != null)
                            throw new NotImplementedException("Area-based SetData on block-compressed texture is currently unsupported. Sorry!");

                        pipe.Context.UpdateSubresource(box, _resource, subLevel);
                    }
                    else
                    {
                        if (area != null)
                        {
                            Rectangle rect = area.Value;
                            int areaPitch = stride * rect.Width;
                            DataBox box = new DataBox(dataPtr, areaPitch, data.Length);

                            ResourceRegion region = new ResourceRegion();
                            region.Top = rect.Y;
                            region.Front = 0;
                            region.Back = 1;
                            region.Bottom = rect.Bottom;
                            region.Left = rect.X;
                            region.Right = rect.Right;
                            pipe.Context.UpdateSubresource(box, _resource, subLevel, region);
                        }
                        else
                        {
                            int x = 0;
                            int y = 0;
                            int w = Math.Max(_width >> mipLevel, 1);
                            int h = Math.Max(_height >> mipLevel, 1);

                            DataBox box = new DataBox(dataPtr, pitch, sliceBytes);

                            ResourceRegion region = new ResourceRegion();
                            region.Top = y;
                            region.Front = 0;
                            region.Back = 1;
                            region.Bottom = y + h;
                            region.Left = x;
                            region.Right = x + w;

                            pipe.Context.UpdateSubresource(box, _resource, subLevel, region);
                        }
                    }
                }
            });
        }

        internal void CopyTo(GraphicsPipe pipe, TextureBase destination)
        {
            if (destination.HasFlags(TextureFlags.Dynamic))
                throw new TextureCopyException(this, destination, "Cannot copy to a dynamic texture via GPU. GPU cannot write to dynamic textures.");

            // Validate dimensions.
            if (destination._width != this._width ||
                destination._height != this._height ||
                destination._depth != this._depth)
                throw new TextureCopyException(this, destination, "The source and destination textures must have the same dimensions.");

            ApplyChanges(pipe);
            destination.ApplyChanges(pipe);

            pipe.Context.CopyResource(this._resource, destination._resource);
        }

        /// <summary>Copies a single slice or mip-map level from one texture to another.</summary>
        /// <param name="pipe">The graphics pipe that will perform the copy.</param>
        /// <param name="sourceLevel">The source mip-map level.</param>
        /// <param name="sourceSlice">The source array slice.</param>
        /// <param name="destination">The destination texture.</param>
        /// <param name="destLevel">The destination mip-map level.</param>
        /// <param name="destSlice">The destination array slice.</param>
        internal void CopyTo(GraphicsPipe pipe, int sourceLevel, int sourceSlice, TextureBase destination, int destLevel, int destSlice)
        {
            if (destination.HasFlags(TextureFlags.Dynamic))
                throw new TextureCopyException(this, destination, "Cannot copy to a dynamic texture via GPU. GPU cannot write to dynamic textures.");
            
            // Validate dimensions.
            if (destination._width != this._width ||
                destination._height != this._height ||
                destination._depth != this._depth)
                throw new TextureCopyException(this, destination, "The source and destination textures must have the same dimensions.");

            if (sourceLevel >= this.MipMapLevels)
                throw new TextureCopyException(this, destination, "The source mip-map level exceeds the total number of levels in the source texture.");

            if(sourceSlice >= this.ArraySize)
                throw new TextureCopyException(this, destination, "The source array slice exceeds the total number of slices in the source texture.");

            if (destLevel >= destination.MipMapLevels)
                throw new TextureCopyException(this, destination, "The destination mip-map level exceeds the total number of levels in the destination texture.");

            if (destSlice >= destination.ArraySize)
                throw new TextureCopyException(this, destination, "The destination array slice exceeds the total number of slices in the destination texture.");

            int srcSub = (sourceSlice * this.MipMapLevels) + sourceLevel;
            int destSub = (destSlice * destination.MipMapLevels) + destLevel;

            ApplyChanges(pipe);
            destination.ApplyChanges(pipe);

            pipe.Context.CopySubresourceRegion(_resource, srcSub, null, destination._resource, destSub);
        }

        /// <summary>Returns the data contained within a texture via a staging texture or directly from the texture itself if possible.</summary>
        /// <param name="stagingTexture">A staging texture to use when retrieving data from the GPU. Only textures
        /// with the staging flag set will work.</param>
        public TextureData GetData(ITexture stagingTexture)
        {
            return GetData(Device.ExternalContext, stagingTexture as TextureBase);
        }

        /// <summary>Returns the data from a single mip-map level within a slice of the texture. For 2D, non-array textures, this will always be slice 0.</summary>
        /// <param name="stagingTexture">The staging texture to copy the data to, from the GPU.</param>
        /// <param name="level">The mip-map level to retrieve.</param>
        /// <param name="arraySlice">The array slice to access.</param>
        /// <returns></returns>
        public TextureData.Slice GetData(ITexture stagingTexture, int level, int arraySlice)
        {
            return GetData(Device.ExternalContext, stagingTexture as TextureBase, level, arraySlice);
        }


        /// <summary>Gets the block-compressed byte size of a mip-map level</summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="blockSize">The number of bytes per block.</param>
        /// <returns></returns>
        private int GetBCSize(int width, int height, int blockSize)
        {
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;

            int numBlocksWide = Math.Max(1, blockCountX);
            int numBlocksHigh = Math.Max(1, blockCountY);

            int numRows = numBlocksHigh;

            int blockPitch = numBlocksWide * blockSize;

            return blockPitch * numRows;
        }

        /// <summary>Gets the block-compressed pitch size of a mip-map level</summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="blockSize">The number of bytes per block.</param>
        /// <returns></returns>
        private int GetBCPitch(int width, int height, int blockSize)
        {
            int blockCountX = (width + 3) / 4;
            int numBlocksWide = Math.Max(1, blockCountX);
            int blockPitch = numBlocksWide * blockSize;

            return blockPitch;
        }

        /// <summary>Gets the block-compressed size of a mip-map level, in bytes.</summary>
        /// <param name="width">The width of the level.</param>
        /// <param name="height">The height of the level.</param>
        /// <param name="blockSize">The block size of the compression format.</param>
        /// <returns></returns>
        private int GetBCLevelSize(int width, int height, int blockSize)
        {
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;

            int numBlocksWide = Math.Max(1, blockCountX);
            int numBlocksHigh = Math.Max(1, blockCountY);
            int numRows = numBlocksHigh;

            int blockPitch = numBlocksWide * blockSize;

            int pitch = width * 4;
            return blockPitch * numRows;
        }

        /// <summary>Immediately changes the size of the underlying texture resource.</summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <param name="newDepth">The number of slices on a 3D texture.</param>
        /// <param name="newArraySize">The number of array slices in an array texture. This will likely be ignored by non-array textures.</param>
        internal void SetSize(int newWidth, int newHeight, int newDepth, int newArraySize)
        {
            BeforeResize();
            _width = Math.Max(1, newWidth);
            _height = Math.Max(1, newHeight);
            _depth = Math.Max(1, newDepth);

            OnSetSize(_width, _height, _depth, Math.Max(1, newArraySize));
            CreateTexture(true);
            AfterResize();
        }

        protected virtual void BeforeResize() { }

        protected virtual void AfterResize() { }

        protected virtual void OnSetSize(int newWidth, int newHeight, int newDepth, int newArraySize) { }

        protected abstract Resource CreateTextureInternal(bool isResizing);

        internal void QueueChange(ITextureChange change)
        {
            _pendingChanges.Enqueue(change);
        }

        /// <summary>Applies all pending changes to the texture. Take care when calling this method in multi-threaded code. Calling while the
        /// GPU may be using the texture will cause unexpected behaviour.</summary>
        /// <param name="pipe"></param>
        internal virtual void ApplyChanges(GraphicsPipe pipe)
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
            ApplyChanges(pipe);
        }

        protected virtual void OnCreateUAV() { }

        /// <summary>Gets the flags that were passed in when the texture was created.</summary>
        public TextureFlags Flags { get { return _flags; } }

        /// <summary>Gets the format of the texture.</summary>
        public Format DxFormat { get { return _format; } }

        public GraphicsFormat Format { get { return (GraphicsFormat)_format; } }

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        public bool IsBlockCompressed { get { return _isBlockCompressed; } }

        /// <summary>Gets the width of the texture.</summary>
        public int Width { get { return _width; } }

        /// <summary>Gets the height of the texture.</summary>
        public int Height { get { return _height; } }

        /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
        public int Depth
        {
            get { return _depth; }
        }

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public abstract int MipMapLevels { get; }

        /// <summary>Gets the number of array slices in the texture.</summary>
        public abstract int ArraySize { get; }

        /// <summary>Gets whether or not the texture is a texture array.</summary>
        public abstract bool IsTextureArray { get; }

        internal GraphicsDevice Device => _device;

        public bool IsValid { get; protected set; }

        internal override ShaderResourceView SRV
        {
            get => _srv;
            set => _srv = value;
        }

        internal override UnorderedAccessView UAV
        {
            get => _uav;
            set => _uav = value;
        }
    }
}
