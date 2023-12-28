using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    public delegate void TextureEvent(TextureDX11 texture);

    public unsafe abstract partial class TextureDX11 : GraphicsTexture
    {
        ResourceHandleDX11<ID3D11Resource>[] _handles;
        ResourceHandleDX11<ID3D11Resource> _curHandle;

        ShaderResourceViewDesc1 _srvDesc;
        UnorderedAccessViewDesc1 _uavDesc;

        /// <summary>
        /// A list of handles that need to be disposed once the GPU is finished with them.
        /// </summary>
        List<ResourceHandleDX11<ID3D11Resource>> _oldHandles;

        internal TextureDX11(DeviceDX11 device, GraphicsTextureType type, 
            TextureDimensions dimensions, 
            AntiAliasLevel aaLevel, 
            MSAAQuality sampleQuality, 
            GraphicsFormat format, 
            GraphicsResourceFlags flags, 
            bool allowMipMapGen, 
            string name) :
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags | GraphicsResourceFlags.GpuRead, allowMipMapGen, name)
        {
            Device = device;
            _oldHandles = new List<ResourceHandleDX11<ID3D11Resource>>();
        }

        protected void SetDebugName(ID3D11Resource* resource, string debugName)
        {
            if (!string.IsNullOrWhiteSpace(debugName))
            {
                void* ptrName = (void*)SilkMarshal.StringToPtr(debugName, NativeStringEncoding.LPStr);
                resource->SetPrivateData(ref RendererDX11.WKPDID_D3DDebugObjectName, (uint)debugName.Length, ptrName);
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
            FreeOldHandles(frameID);
        }

        protected void FreeOldHandles(ulong frameID)
        {
            // Dispose of old texture handles from any previous resize calls.
            uint resizeAge = (uint)(frameID - LastFrameResizedID);
            if (resizeAge > Device.FrameBufferSize)
            {
                foreach (ResourceHandleDX11<ID3D11Resource> handle in _oldHandles)
                    handle.Dispose();

                _oldHandles.Clear();
            }
        }

        protected virtual ResourceHandleDX11<ID3D11Resource> CreateHandle()
        {
            return new ResourceHandleDX11<ID3D11Resource>(this);
        }

        protected override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            _handles = new ResourceHandleDX11<ID3D11Resource>[frameBufferSize];

            if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                SetSRVDescription(ref _srvDesc);

            if(Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                SetUAVDescription(ref _srvDesc, ref _uavDesc);

            for (uint i = 0; i < frameBufferSize; i++)
            {
                ResourceHandleDX11<ID3D11Resource> handle = CreateHandle();
                _handles[i] = handle;
                CreateTexture(Device, handle, i);

                SetDebugName(handle.NativePtr, $"{Name}_FI{i}");

                if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                {
                    handle.SRV.Desc = _srvDesc;
                    handle.SRV.Create();
                }

                if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                {
                    handle.UAV.Desc = _uavDesc;
                    handle.UAV.Create();
                }
            }

            _curHandle = _handles[frameBufferIndex];
        }

        protected abstract void CreateTexture(DeviceDX11 device, ResourceHandleDX11<ID3D11Resource> handle, uint handleIndex);

        protected override void OnResizeTexture(in TextureDimensions dimensions, GraphicsFormat format, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            UpdateDescription(dimensions, format);
            Dimensions = dimensions;

            _oldHandles.AddRange(_handles);
            OnCreateResource(frameBufferSize, frameBufferIndex, frameID);
            _curHandle = _handles[frameBufferIndex];
        }

        protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            OnResizeTexture(Dimensions, ResourceFormat, frameBufferSize, frameBufferIndex, frameID);
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

        protected override void OnGraphicsRelease()
        {
            for (int i = 0; i < _handles.Length; i++)
                _handles[i].Dispose();
        }

        protected override void OnGenerateMipMaps(GraphicsQueue cmd)
        {
            if (_curHandle.Ptr != null)
                (cmd as GraphicsQueueDX11).Ptr->GenerateMips(_curHandle.SRV);
        }

        protected abstract void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat);

        /// <summary>Gets the format of the texture.</summary>
        public Format DxgiFormat => ResourceFormat.ToApi();

        public GraphicsFormat DataFormat => (GraphicsFormat)DxgiFormat;

        public override ResourceHandleDX11<ID3D11Resource> Handle => _curHandle;

        public override unsafe void* SRV => _curHandle.SRV.Ptr;

        public override unsafe void* UAV => _curHandle.UAV.Ptr;

        public new DeviceDX11 Device { get; }
    }
}
