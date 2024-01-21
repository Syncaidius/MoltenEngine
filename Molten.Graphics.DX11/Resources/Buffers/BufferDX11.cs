using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Runtime.InteropServices;

namespace Molten.Graphics.DX11;

/// <summary>
/// The standard implementation of <see cref="GraphicsBuffer"/> for DirectX 11. Also acts as a base class for other buffer types.
/// </summary>
public unsafe class BufferDX11 : GraphicsBuffer
{
    ResourceHandleDX11<ID3D11Buffer> _handle;
    protected BufferDesc Desc;

    void* _initialData;
    uint _initialBytes;

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
        uint alignment,
        void* initialData,
        uint initialBytes) : base(device, stride, numElements, flags | GraphicsResourceFlags.GpuRead, type, alignment)
    {
        if (initialData != null && initialBytes > 0)
        {
            _initialData = EngineUtil.Alloc(initialBytes);
            _initialBytes = initialBytes;
            NativeMemory.Copy(initialData, _initialData, _initialBytes);
        }

        ResourceFormat = format;
        D3DFormat = format.ToApi();

        device.ProcessDebugLayerMessages();
    }

    protected override GraphicsBuffer OnAllocateSubBuffer(ulong offset, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type, uint alignment)
    {
        throw new NotImplementedException();
    }

    protected override sealed void OnCreateResource()
    {
        DeviceDX11 device = Device as DeviceDX11;
        _handle = new ResourceHandleDX11<ID3D11Buffer>(this);

        if (Flags.IsImmutable() && _initialData == null)
            throw new GraphicsResourceException(this, "Initial data cannot be null when buffer mode is Immutable.");

        Desc = new BufferDesc();
        Desc.ByteWidth = (uint)SizeInBytes;
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

        CreateBuffer(device, _handle);
    }

    private void CreateBuffer(DeviceDX11 device, ResourceHandleDX11<ID3D11Buffer> handle)
    {
        if (_initialData != null)
        {
            SubresourceData srd = new SubresourceData(_initialData, _initialBytes, (uint)SizeInBytes);
            fixed (BufferDesc* pDesc = &Desc)
                device.Ptr->CreateBuffer(pDesc, &srd, ref handle.NativePtr);

            EngineUtil.Free(ref _initialData);
            _initialBytes = 0;
        }
        else
        {
            fixed (BufferDesc* pDesc = &Desc)
                device.Ptr->CreateBuffer(pDesc, null, ref handle.NativePtr);
        }

        CreateViews(device, handle);
        Version++;
    }

    protected virtual void CreateViews(DeviceDX11 device, ResourceHandleDX11<ID3D11Buffer> handle)
    {
        // Create shader resource view (SRV), if shader access is permitted.
        if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
        {
            handle.SRV.Desc = new ShaderResourceViewDesc1()
            {
                Buffer = new BufferSrv()
                {
                    NumElements = (uint)ElementCount,
                    FirstElement = 0,
                },
                ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                Format = Format.FormatUnknown,
            };

            handle.SRV.Create();
        }

        // Create unordered access view (UAV), if unordered access is permitted.
        if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
        {
            handle.UAV.Desc = new UnorderedAccessViewDesc1()
            {
                Format = Format.FormatUnknown,
                ViewDimension = UavDimension.Buffer,
                Buffer = new BufferUav()
                {
                    NumElements = (uint)ElementCount,
                    FirstElement = 0,
                    Flags = 0, // TODO add support for append, raw and counter buffers. See: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_buffer_uav_flag
                }
            };
            handle.UAV.Create();
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
        _handle?.Dispose();
        _handle = null;

        // Just in case the initial data was never used before disposal.
        EngineUtil.Free(ref _initialData);
    }

    /// <summary>Gets the resource usage flags associated with the buffer.</summary>
    internal ResourceMiscFlag ResourceFlags => (ResourceMiscFlag)Desc.MiscFlags;

    /// <inheritdoc/>
    public override ResourceHandleDX11<ID3D11Buffer> Handle => _handle;

    /// <inheritdoc/>
    public override GraphicsFormat ResourceFormat { get; protected set; }

    internal Format D3DFormat { get; }
}
