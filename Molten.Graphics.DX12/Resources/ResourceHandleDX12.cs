using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class ResourceHandleDX12 : GraphicsResourceHandle
{
    ID3D12Resource1* _ptr;

    internal ResourceHandleDX12(GraphicsResource resource, ID3D12Resource1* ptr) : base(resource)
    {
        _ptr = ptr;
        Device = resource.Device as DeviceDX12;
        SRV = new ViewDX12<ShaderResourceViewDesc>(this);
        UAV = new ViewDX12<UnorderedAccessViewDesc>(this);
    }

    public override void Dispose()
    {
        NativeUtil.ReleasePtr(ref _ptr);
    }

    public static implicit operator ID3D12Resource1*(ResourceHandleDX12 handle)
    {
        return handle._ptr;
    }

    public static implicit operator ID3D12Resource*(ResourceHandleDX12 handle)
    {
        return (ID3D12Resource*)handle._ptr;
    }

    protected void SetHandle(ID3D12Resource1* ptr)
    {
        _ptr = ptr;
    }

    internal ViewDX12<ShaderResourceViewDesc> SRV { get; }

    internal ViewDX12<UnorderedAccessViewDesc> UAV { get; }

    internal DeviceDX12 Device { get; }

    public unsafe ID3D12Resource1* Ptr1 => _ptr;

    public unsafe ID3D12Resource* Ptr => (ID3D12Resource*)_ptr;
}

public class ResourceHandleDX12<D> : ResourceHandleDX12
    where D : unmanaged
{
    internal unsafe ResourceHandleDX12(GraphicsResource resource, ID3D12Resource1* ptr) :
        base(resource, ptr)
    {
        View = new ViewDX12<D>(this);
    }

    internal unsafe ResourceHandleDX12(GraphicsResource resource, ID3D12Resource1* ptr, ref D desc) :
        base(resource, ptr)
    {
        View = new ViewDX12<D>(this)
        {
            Desc = desc,
        };
    }

    /// <summary>
    /// An additional, unique view of the resource, with a specific description.
    /// </summary>
    internal ViewDX12<D> View { get; }
}
