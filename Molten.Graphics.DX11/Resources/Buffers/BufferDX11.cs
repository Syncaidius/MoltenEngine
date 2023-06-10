using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    /// <summary>
    /// The standard implementation of <see cref="GraphicsBuffer"/> for DirectX 11. Also acts as a base class for other buffer types.
    /// </summary>
    public unsafe class BufferDX11 : GraphicsBuffer
    {
        ID3D11Buffer* _native;
        uint _ringPos;
        protected BufferDesc Desc;

        /// <summary>
        /// Creates a new instance of <see cref="BufferDX11"/> with the specified parameters.
        /// </summary>
        /// <param name="device">The <see cref="DeviceDX11"/> that the buffer will be bound to.</param>
        /// <param name="type">The buffer type.</param>
        /// <param name="flags">The flags to use during initialization.</param>
        /// <param name="stride">The stride or bytes-per-element of the buffer.</param>
        /// <param name="numElements">The maximum number of elements that the buffer can hold.</param>
        /// <param name="initialData">An optional pointer to data to be copied to the buffer during initialization. If <paramref name="initialBytes"/> is 0, this parameter is ignored.</param>
        /// <param name="initialBytes">The initial size of <paramref name="initialData"/>, in bytes. If <paramref name="initialData"/> is null, this parameter is ignored.</param>
        internal BufferDX11(DeviceDX11 device,
            GraphicsBufferType type,
            GraphicsResourceFlags flags,
            GraphicsFormat format,
            uint stride,
            uint numElements,
            void* initialData,
            uint initialBytes) : base(device, stride, numElements, flags | GraphicsResourceFlags.GpuRead, type)
        {
            ResourceFormat = format;
            D3DFormat = format.ToApi();
            NativeSRV = new SRView(this);
            NativeUAV = new UAView(this);

            InitializeBuffer(initialData, initialBytes);
            device.ProcessDebugLayerMessages();
        }

        internal void SetVertexFormat(VertexFormat format)
        {
            VertexFormat = format;
        }

        /// <summary>
        /// Initializes the current instance of <see cref="BufferDX11"/>.
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

            if (initialData != null && initialBytes > 0)
            {
                SubresourceData srd = new SubresourceData(initialData, initialBytes, SizeInBytes);
                fixed(BufferDesc* pDesc = &Desc)
                    nDevice.Ptr->CreateBuffer(pDesc, &srd, ref _native);
            }
            else
            {
                fixed (BufferDesc* pDesc = &Desc)
                    nDevice.Ptr->CreateBuffer(pDesc, null, ref _native);
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

        /// <inheritdoc/>
        protected override void OnGraphicsRelease()
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

        /// <inheritdoc/>
        public override unsafe void* Handle => _native;

        /// <inheritdoc/>
        public override unsafe void* SRV => NativeSRV.Ptr;

        /// <inheritdoc/>
        public override unsafe void* UAV => NativeUAV.Ptr;

        internal SRView NativeSRV { get; }

        internal UAView NativeUAV { get; }

        /// <inheritdoc/>
        public override GraphicsFormat ResourceFormat { get; protected set; }

        internal Format D3DFormat { get; }
    }
}
