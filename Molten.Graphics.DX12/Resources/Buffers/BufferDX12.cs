using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12;

public class BufferDX12 : GraphicsBuffer
{
    ResourceHandleDX12 _handle;
    ResourceBarrier _barrier;
    ResourceStates _barrierState;

    internal BufferDX12(DeviceDX12 device, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type, uint alignment) :
        base(device, stride, numElements, flags, type, alignment)
    {
        Device = device;
    }

    private BufferDX12(BufferDX12 parentBuffer, ulong offset, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type, uint alignment)
        : base(parentBuffer.Device, stride, numElements, flags, type, alignment)
    {
        if (ParentBuffer != null)
        {
            ParentBuffer = parentBuffer;
            RootBuffer = parentBuffer.RootBuffer ?? parentBuffer;
        }
        Offset = offset;
    }

    protected unsafe override void OnCreateResource()
    {
        _handle?.Dispose();

        HeapFlags heapFlags = HeapFlags.None;
        ResourceFlags flags = Flags.ToResourceFlags();
        HeapType heapType = Flags.ToHeapType();
        ResourceStates stateFlags = Flags.ToResourceState();
        if (ParentBuffer == null)
        {
            HeapProperties heapProp = new HeapProperties()
            {
                Type = HeapType.Default, // heapType, // TODO Properly set heap properties based on access flags and UMA support.
                CPUPageProperty = CpuPageProperty.Unknown,
                CreationNodeMask = 1,
                MemoryPoolPreference = MemoryPool.Unknown,
                VisibleNodeMask = 1,
            };

            // TODO Adjust for GPU memory architecture based on UMA support.
            // See for differences: https://microsoft.github.io/DirectX-Specs/d3d/D3D12GPUUploadHeaps.html
            if (heapType == HeapType.Custom)
            {
                // TODO Set CPUPageProperty and MemoryPoolPreference based on UMA support.
            }

            if (Flags.Has(GraphicsResourceFlags.DenyShaderAccess))
                flags |= ResourceFlags.DenyShaderResource;

            if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                flags |= ResourceFlags.AllowUnorderedAccess;

            ResourceDesc1 desc = new()
            {
                Dimension = ResourceDimension.Buffer,
                Alignment = 0,
                Width = SizeInBytes,
                Height = 1,
                DepthOrArraySize = 1,
                Layout = TextureLayout.LayoutRowMajor,
                Format = ResourceFormat.ToApi(),
                Flags = flags,
                MipLevels = 1,
                SampleDesc = new SampleDesc(1, 0),
            };

            Guid guid = ID3D12Resource1.Guid;
            void* ptr = null;
            HResult hr = Device.Handle->CreateCommittedResource2(heapProp, heapFlags, desc, stateFlags, null, null, &guid, &ptr);
            if (!Device.Log.CheckResult(hr, () => $"Failed to create {desc.Dimension} resource"))
                return;

            _handle = new ResourceHandleDX12(this, (ID3D12Resource1*)ptr);
        }
        else
        {
            _handle = OnCreateHandle((ID3D12Resource1*)RootBuffer.Handle.Ptr);
        }
    }

    private unsafe ResourceHandleDX12 OnCreateHandle(ID3D12Resource1* ptr)
    {
        switch (BufferType)
        {
            case GraphicsBufferType.Vertex:
                _handle = new VBHandleDX12(this, ptr);
                break;

            case GraphicsBufferType.Index:
                _handle = new IBHandleDX12(this, ptr);
                break;

            case GraphicsBufferType.Constant:
                CBHandleDX12 cbHandle = new(this, ptr);
                ConstantBufferViewDesc cbDesc = new()
                {
                    BufferLocation = ptr->GetGPUVirtualAddress() + Offset,
                    SizeInBytes = (uint)SizeInBytes,
                };

                cbHandle.CBV.Initialize(ref cbDesc);
                _handle = cbHandle;
                break;

            default:
                _handle = new ResourceHandleDX12(this, ptr);
                break;
        }

        // TODO Constant buffers must be 256-bit aligned, which means the data sent to SetData() must be too.
        //      We can validate this by checking if the stride is a multiple of 256: sizeof(T) % 256 == 0
        //      This value is also provided via D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT.
        //      
        //      If not, we throw an exception stating this.

        if (!Flags.Has(GraphicsResourceFlags.DenyShaderAccess))
        {
            ShaderResourceViewDesc desc = new()
            {
                Format = ResourceFormat.ToApi(),
                ViewDimension = SrvDimension.Buffer,
                Shader4ComponentMapping = (uint)ShaderComponentMapping.ForceValue1,
                Buffer = new BufferSrv()
                {
                    FirstElement = Stride > 0 ? (Offset / Stride) : 0,
                    NumElements = (uint)ElementCount,
                    Flags = BufferSrvFlags.None,
                    StructureByteStride = Stride, // TODO If stride is 0, then it is a typed buffer, where the ResourceFormat must be set to a valid format.
                },
            };

            _handle.SRV.Initialize(ref desc);
        }

        if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
        {
            UnorderedAccessViewDesc desc = new()
            {
                Format = ResourceFormat.ToApi(),
                ViewDimension = UavDimension.Buffer,
                Buffer = new BufferUav()
                {
                    FirstElement = Stride > 0 ? (Offset / Stride) : 0,
                    NumElements = (uint)ElementCount,
                    Flags = BufferUavFlags.None,
                    CounterOffsetInBytes = 0,
                    StructureByteStride = Stride, // TODO If stride is 0, then it is a typed buffer, where the ResourceFormat must be set to a valid format.
                },
            };

            _handle.UAV.Initialize(ref desc);
        }

        return _handle;
    }

    protected override GraphicsBuffer OnAllocateSubBuffer(
        ulong offset, 
        uint stride, 
        ulong numElements, 
        GraphicsResourceFlags flags,
        GraphicsBufferType type,
        uint alignment)
    {
        // TODO check through existing allocations to see if we can re-use one.
        return new BufferDX12(this, offset, stride, numElements, Flags, BufferType, alignment);
    }

    protected override void OnGraphicsRelease()
    {
        _handle?.Dispose();
    }

    /// <inheritdoc/>
    public override ResourceHandleDX12 Handle => _handle;

    /// <inheritdoc/>
    public override GraphicsFormat ResourceFormat { get; protected set; }

    public new DeviceDX12 Device { get; }

    /// <summary>
    /// Gets the root <see cref="BufferDX12"/> instance. This is the top-most buffer, regardless of how many nested sub-buffers we allocated.
    /// </summary>
    internal BufferDX12 RootBuffer { get; private set; }

    /// <summary>
    /// Gets the internal resource barrier state of the current <see cref="BufferDX12"/>.
    /// </summary>
    internal ResourceStates BarrierState { get; set; }
}
