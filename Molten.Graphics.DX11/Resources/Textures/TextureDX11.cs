using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public delegate void TextureEvent(TextureDX11 texture);

    public unsafe abstract partial class TextureDX11 : GraphicsTexture
    {
        /// <summary>Triggered right before the internal texture resource is created.</summary>
        public event TextureEvent OnPreCreate;

        /// <summary>Triggered after the internal texture resource has been created.</summary>
        public event TextureEvent OnCreate;

        /// <summary>Triggered if the creation of the internal texture resource has failed (resulted in a null resource).</summary>
        public event TextureEvent OnCreateFailed;

        ID3D11Resource* _native;
        SRView _srv;
        UAView _uav;

        internal TextureDX11(GraphicsDevice device, uint width, uint height, uint depth, uint mipCount, 
            uint arraySize, AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) :
            base(device, width, height, depth, mipCount, arraySize, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
            _srv = new SRView(this);
            _uav = new UAView(this);
        }

        protected void SetDebugName(string debugName)
        {
            if (!string.IsNullOrWhiteSpace(debugName))
            {
                void* ptrName = (void*)SilkMarshal.StringToPtr(debugName, NativeStringEncoding.LPStr);
                ((ID3D11Resource*)Handle)->SetPrivateData(ref RendererDX11.WKPDID_D3DDebugObjectName, (uint)debugName.Length, ptrName);
                SilkMarshal.FreeString((nint)ptrName, NativeStringEncoding.LPStr);
            }
        }

        protected BindFlag GetBindFlags()
        {
            BindFlag result = Flags.ToBindFlags();

            if (this is RenderSurface2DDX11)
                result |= BindFlag.RenderTarget;

            if (this is DepthSurfaceDX11)
                result |= BindFlag.DepthStencil;

            return result;
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
                if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                {
                    SetSRVDescription(ref _srv.Desc);
                    _srv.Create();
                }

                if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                {
                    SetUAVDescription(ref _srv.Desc, ref _uav.Desc);
                    _uav.Create();
                }

                Version++;
                OnCreate?.Invoke(this);
            }
            else
            {
                OnCreateFailed?.Invoke(this);
            }
        }

        protected abstract void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc);

        protected abstract void SetSRVDescription(ref ShaderResourceViewDesc1 desc);

        protected virtual void OnDisposeForRecreation()
        {
            GraphicsRelease();
        }

        public override void GraphicsRelease()
        {
            _srv.Release();
            _uav.Release();

            //TrackDeallocation();
            SilkUtil.ReleasePtr(ref _native);
        }

        /// <summary>Generates mip maps for the texture via the provided <see cref="GraphicsQueueDX11"/>.</summary>
        public void GenerateMipMaps(GraphicsPriority priority)
        {
            if (!IsMipMapGenAllowed)
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
            QueueTask(priority, new TextureSetTask<byte>(data.Data, 0, data.TotalBytes)
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

        public void SetData<T>(GraphicsPriority priority, uint level, T* data, uint startIndex, uint count, uint pitch, uint arrayIndex, Action<GraphicsResource> completeCallback = null)
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
                Staging = stagingTexture as TextureDX11,
                CompleteCallback = callback,
                ArrayIndex = arrayIndex,
                MipMapLevel = mipLevel,
            });
        }

        protected override void OnSetSize()
        {
            UpdateDescription(Width, Height, Depth, Math.Max(1, MipMapCount), Math.Max(1, ArraySize), DxgiFormat);
            CreateTexture(true);
        }


        protected virtual void UpdateDescription(uint newWidth, uint newHeight, 
            uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat) { }

        protected abstract ID3D11Resource* CreateResource(bool resize);

        /// <summary>Applies all pending changes to the texture. Take care when calling this method in multi-threaded code. Calling while the
        /// GPU may be using the texture will cause unexpected behaviour.</summary>
        /// <param name="cmd"></param>
        protected override void OnApply(GraphicsQueue cmd)
        {
            if (IsDisposed)
                return;

            if(_native == null)
                CreateTexture(false);

            base.OnApply(cmd);
        }

        /// <summary>Gets the format of the texture.</summary>
        public Format DxgiFormat => ResourceFormat.ToApi();

        public GraphicsFormat DataFormat => (GraphicsFormat)DxgiFormat;

        public override unsafe void* Handle => _native;

        public override unsafe void* SRV => _srv.Ptr;

        public override unsafe void* UAV => _uav.Ptr;
    }
}
