using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class BufferDX11 : GraphicsBuffer
    {
        ID3D11Buffer* _native;
        uint _ringPos;
        internal BufferDesc Desc;

        internal BufferDX11(DeviceDX11 device,
            GraphicsBufferType type,
            GraphicsResourceFlags flags,
            uint stride,
            uint numElements,
            void* initialData,
            uint initialBytes) : base(device, stride, numElements, flags, type)
        {
            ResourceFormat = GraphicsFormat.Unknown;
            NativeSRV = new SRView(this);
            NativeUAV = new UAView(this);

            InitializeBuffer(initialData, initialBytes);
            device.ProcessDebugLayerMessages();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialDataPtr">A pointer to data that the buffer should initially be populated with.</param>
        protected virtual void InitializeBuffer(void* initialData, uint initialBytes)
        {
            DeviceDX11 nDevice = Device as DeviceDX11;
            if (Flags.IsImmutable() && initialData == null)
                throw new GraphicsResourceException(this, "Initial data cannot be null when buffer mode is Immutable.");

            Desc = new BufferDesc();
            Desc.ByteWidth = SizeInBytes;
            Desc.StructureByteStride = 0;
            Desc.Usage = Flags.ToUsageFlags();
            Desc.CPUAccessFlags = (uint)Flags.ToCpuFlags();

            // Only staging allows CPU reads.
            // See for ref: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_usage
            if (Desc.Usage == Usage.Staging)
            {
                Desc.MiscFlags = 0U;
                Desc.BindFlags = 0U;
            }
            else
            {
                Desc.BindFlags = (uint)(Flags.ToBindFlags() | BufferType.ToBindFlags());
                Desc.MiscFlags = (uint)BufferType.ToMiscFlags();

                if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                    Desc.BindFlags |= (uint)BindFlag.ShaderResource;

                if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                    Desc.BindFlags |= (uint)BindFlag.UnorderedAccess;
            }

            // Ensure structured buffers get the stride info.
            if (Desc.MiscFlags == (uint)ResourceMiscFlag.BufferStructured)
                Desc.StructureByteStride = Stride;

            if (initialData != null)
            {
                SubresourceData srd = new SubresourceData(initialData, initialBytes, SizeInBytes);
                nDevice.Ptr->CreateBuffer(ref Desc, ref srd, ref _native);
            }
            else
            {
                nDevice.Ptr->CreateBuffer(ref Desc, null, ref _native);
            }

            Device.AllocateVRAM(SizeInBytes);
            CreateResources();
        }

        protected virtual void CreateResources()
        {
            if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
            {
                NativeSRV.Desc = new ShaderResourceViewDesc1()
                {
                    Buffer = new BufferSrv()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                    Format = Format.FormatUnknown,
                };

                NativeSRV.Create();
            }

            if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
            {
                NativeUAV.Desc = new UnorderedAccessViewDesc1()
                {
                    Format = Format.FormatUnknown,
                    ViewDimension = UavDimension.Buffer,
                    Buffer = new BufferUav()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                        Flags = 0, // TODO add support for append, raw and counter buffers. See: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_buffer_uav_flag
                    }
                };
                NativeUAV.Create();
            }
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

        public override void GraphicsRelease()
        {
            NativeSRV.Release();
            NativeUAV.Release();

            if (Handle != null)
            {
                SilkUtil.ReleasePtr(ref _native);
                Device.DeallocateVRAM(Desc.ByteWidth);
            }
        }

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        internal ResourceMiscFlag ResourceFlags => (ResourceMiscFlag)Desc.MiscFlags;

        public override unsafe void* Handle => _native;

        public override unsafe void* SRV => NativeSRV.Ptr;

        public override unsafe void* UAV => NativeUAV.Ptr;

        internal SRView NativeSRV { get; }

        internal UAView NativeUAV { get; }

        public override GraphicsFormat ResourceFormat { get; protected set; }
    }
}
