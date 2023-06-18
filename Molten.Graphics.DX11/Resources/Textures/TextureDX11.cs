using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
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

        ResourceHandleDX11<ID3D11Resource>[] _handles;
        ResourceHandleDX11<ID3D11Resource> _curHandle;

        internal TextureDX11(GraphicsDevice device, GraphicsTextureType type, 
            TextureDimensions dimensions, 
            AntiAliasLevel aaLevel, 
            MSAAQuality sampleQuality, 
            GraphicsFormat format, 
            GraphicsResourceFlags flags, 
            bool allowMipMapGen, 
            string name) :
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags | GraphicsResourceFlags.GpuRead, allowMipMapGen, name)
        {
            
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

        protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
        {
            _curHandle = _handles[frameBufferIndex];
        }

        protected override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            throw new NotImplementedException();
        }

        protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            throw new NotImplementedException();
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

        protected SubresourceData* GetImmutableData(Usage usage)
        {
            SubresourceData* subData = null;

            // Check if we're passing initial data to the texture.
            // Render surfaces and depth-stencil buffers cannot be initialized with data.
            if (TextureType < GraphicsTextureType.Surface2D)
            {
                // We can only pass data for immutable textures.
                if (usage == Usage.Immutable)
                {
                    subData = EngineUtil.AllocArray<SubresourceData>(MipMapCount * ArraySize);

                    // Immutable textures expect data to be provided via SetData() before other operations are performed on them.
                    for (uint a = 0; a < ArraySize; a++)
                    {
                        for (uint m = 0; m < MipMapCount; m++)
                        {
                            if (!DequeueTaskIfType(out TextureSetTask task))
                                throw new GraphicsResourceException(this, "Immutable texture SetData() was not called or did not provide enough data.");

                            if (task.MipLevel != m || task.ArrayIndex != a)
                                throw new GraphicsResourceException(this, "The provided immutable texture subresource data was not correctly ordered.");

                            uint subIndex = (a * MipMapCount) + m;
                            subData[subIndex] = new SubresourceData(task.Data, task.Pitch, task.NumBytes);
                        }
                    }
                }
            }

            return subData;
        }

        protected abstract void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc);

        protected abstract void SetSRVDescription(ref ShaderResourceViewDesc1 desc);

        protected virtual void OnDisposeForRecreation()
        {
            OnGraphicsRelease();
        }

        protected override void OnGraphicsRelease()
        {
            _srv.Release();
            _uav.Release();

            //TrackDeallocation();
            SilkUtil.ReleasePtr(ref _native);
        }

        protected override void OnSetSize()
        {
            UpdateDescription(Width, Height, Depth, Math.Max(1, MipMapCount), Math.Max(1, ArraySize), DxgiFormat);
            CreateTexture(true);
        }

        protected override void OnGenerateMipMaps(GraphicsQueue cmd)
        {
            if (_srv.Ptr != null)
                (cmd as GraphicsQueueDX11).Ptr->GenerateMips(_srv);
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

        public override ResourceHandleDX11<ID3D11Resource> Handle => _curHandle;

        public override unsafe void* SRV => _srv.Ptr;

        public override unsafe void* UAV => _uav.Ptr;
    }
}
