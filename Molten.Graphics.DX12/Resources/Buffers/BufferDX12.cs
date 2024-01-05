using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12;

public class BufferDX12 : GraphicsBuffer
{
    ResourceHandleDX12 _curHandle;

    public BufferDX12(DeviceDX12 device, uint stride, uint numElements, GraphicsResourceFlags flags, GraphicsBufferType type) : 
        base(device, stride, numElements, flags, type)
    {
        Device = device;
    }

    protected unsafe override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
    {
        _curHandle?.Dispose();

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
            Format = Format.FormatUnknown,
            Flags = flags,
        };

        Guid guid = ID3D12Resource.Guid;
        void* ptr = null;
        HResult hr = Device.Ptr->CreateCommittedResource(heapProp, heapFlags, desc, stateFlags, null, &guid, &ptr);
        if (!Device.Log.CheckResult(hr, () => $"Failed to create {desc.Dimension} resource"))
            return;

        _curHandle = new ResourceHandleDX12(this, (ID3D12Resource*)ptr);
    }

    protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
    {
        throw new NotImplementedException();
    }

    protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
    {
        throw new NotImplementedException();
    }

    protected override void OnGraphicsRelease()
    {
        _curHandle?.Dispose();
    }

    /// <inheritdoc/>
    public override ResourceHandleDX12 Handle => _curHandle;

    /// <inheritdoc/>
    public override GraphicsFormat ResourceFormat { get; protected set; }

    public new DeviceDX12 Device { get; }
}
