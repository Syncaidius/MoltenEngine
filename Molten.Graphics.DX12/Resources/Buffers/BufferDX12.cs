using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public class BufferDX12 : GraphicsBuffer
{
    ResourceHandleDX12 _handle; 
    List<SubBufferDX12> _allocations;

    public BufferDX12(DeviceDX12 device, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type) : 
        base(device, stride, numElements, flags, type)
    {
        Device = device;
        _allocations = new List<SubBufferDX12>();
        ResourceFormat = GraphicsFormat.Unknown;
    }

    protected unsafe override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
    {
        _handle?.Dispose();

        HeapProperties heapProp = new HeapProperties()
        {
            Type = HeapType.Upload,
            CPUPageProperty = CpuPageProperty.Unknown,
            CreationNodeMask = 1,
            MemoryPoolPreference = MemoryPool.Unknown,
            VisibleNodeMask = 1,
        };

        HeapFlags heapFlags = HeapFlags.None;
        ResourceFlags flags = ResourceFlags.None;
        HeapType heapType = Flags.ToHeapType();
        ResourceStates stateFlags = Flags.ToResourceState();

        if(Flags.Has(GraphicsResourceFlags.NoShaderAccess))
            flags |= ResourceFlags.DenyShaderResource;

        if(Flags.Has(GraphicsResourceFlags.UnorderedAccess))
            flags |= ResourceFlags.AllowUnorderedAccess;

        ResourceDesc desc = new ResourceDesc()
        {
            Dimension = ResourceDimension.Buffer,
            Alignment = 0,
            Width = SizeInBytes,
            Height = 1,
            DepthOrArraySize = 1,
            Layout = TextureLayout.LayoutRowMajor,
            Format = ResourceFormat.ToApi(),
            Flags = flags,
        };

        Guid guid = ID3D12Resource.Guid;
        void* ptr = null;
        HResult hr = Device.Ptr->CreateCommittedResource(heapProp, heapFlags, desc, stateFlags, null, &guid, &ptr);
        if (!Device.Log.CheckResult(hr, () => $"Failed to create {desc.Dimension} resource"))
            return;

        _handle = new ResourceHandleDX12(this, (ID3D12Resource*)ptr);
    }

    protected virtual void OnCreateViews()
    {
        // TODO Constant buffers must be 256-bit aligned, which means the data sent to SetData() must be too.
        //      We can validate this by checking if the stride is a multiple of 256: sizeof(T) % 256 == 0
        //      This value is also provided via D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT.
        //      
        //      If not, we throw an exception stating this.

        if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
        {
            _handle.SRV.Desc = new ShaderResourceViewDesc()
            {
                Format = ResourceFormat.ToApi(),
                ViewDimension = SrvDimension.Buffer,
                Shader4ComponentMapping = (uint)(ShaderComponentMapping.FromMemoryComponent0 |
                    ShaderComponentMapping.FromMemoryComponent1 |
                    ShaderComponentMapping.FromMemoryComponent2 |
                    ShaderComponentMapping.FromMemoryComponent3),
                Buffer = new BufferSrv()
                {
                    FirstElement = Stride > 0 ? (Offset / Stride) : 0,
                    NumElements = (uint)ElementCount,
                    Flags = BufferSrvFlags.None,
                    StructureByteStride = Stride, // TODO If stride is 0, then it is a typed buffer, where the ResourceFormat must be set to a valid format.
                },
            };
        }

        if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
        {
            _handle.UAV.Desc = new UnorderedAccessViewDesc()
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
        }
    }
    protected override ulong GetTypeAlignment(GraphicsBufferType type)
    {
        throw new NotImplementedException();
    }


    protected override GraphicsBuffer OnAllocateSubBuffer(ulong offset, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type)
    {
        // TODO check through existing allocations to see if we can re-use one.
        return new SubBufferDX12(this, offset, stride, numElements, Flags, BufferType)
        {
            IsFree = false,
        };
    }

    protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
    {
        // TODO This should be left up to the renderer to handle. E.g. initializing 3 render targets for 3 swapchain buffers.
        //   - Device.Map() should handle memory and resource allocation based on the resoruce flags.
        //     -- E.g. if a resource is dynamic, it should be allocated a new area of memory for each map call.
        //     -- 

        throw new NotImplementedException();
    }

    protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
    {
        throw new NotImplementedException();
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
}
