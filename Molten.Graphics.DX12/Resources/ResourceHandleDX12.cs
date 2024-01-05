using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class ResourceHandleDX12 : GraphicsResourceHandle
{
    ID3D12Resource* _ptr;

    internal ResourceHandleDX12(GraphicsResource resource, ID3D12Resource* ptr) : base(resource)
    {
        _ptr = ptr;
        Device = resource.Device as DeviceDX12;
        SRV = new ResourceViewDX12<ShaderResourceViewDesc>(this);
        UAV = new ResourceViewDX12<UnorderedAccessViewDesc>(this);
    }

    public override void Dispose()
    {
        NativeUtil.ReleasePtr(ref _ptr);
    }

    public static implicit operator ID3D12Resource*(ResourceHandleDX12 handle)
    {
        return handle._ptr;
    }

    internal ResourceViewDX12<ShaderResourceViewDesc> SRV { get; }

    internal ResourceViewDX12<UnorderedAccessViewDesc> UAV { get; }

    internal DeviceDX12 Device { get; }

    public override unsafe void* Ptr => _ptr;
}
