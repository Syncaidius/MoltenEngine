using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class ResourceHandleDX12 : GraphicsResourceHandle
{
    ID3D12Resource1* _ptr;

    internal ResourceHandleDX12(GraphicsResource resource, ID3D12Resource1* ptr) : base(resource)
    {
        _ptr = ptr;
        Device = resource.Device as DeviceDX12;

        if (!resource.Flags.Has(GraphicsResourceFlags.DenyShaderAccess))
            SRV = new SRViewDX12(this);

        if(resource.Flags.Has(GraphicsResourceFlags.UnorderedAccess))
            UAV = new UAViewDX12(this);
    }

    internal void UpdateResource(ID3D12Resource1* ptr)
    {
        _ptr = ptr;
    }

    public override void Dispose()
    {
        SRV?.Dispose();
        UAV?.Dispose();
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

    internal SRViewDX12 SRV { get; }

    internal UAViewDX12 UAV { get; }

    internal DeviceDX12 Device { get; }

    public unsafe ID3D12Resource1* Ptr1 => _ptr;

    public unsafe ID3D12Resource* Ptr => (ID3D12Resource*)_ptr;
}

public class ResourceHandleDX12<V, VD> : ResourceHandleDX12
    where V : ViewDX12<VD>, new()
    where VD : unmanaged
{
    internal unsafe ResourceHandleDX12(GraphicsResource resource, ID3D12Resource1* ptr) :
        base(resource, ptr)
    {
        VD desc = new VD();
        View = new V();
        View.Initialize(this, ref desc);
    }

    internal unsafe ResourceHandleDX12(GraphicsResource resource, ID3D12Resource1* ptr, ref VD desc) :
        base(resource, ptr)
    {
        View = new V();
        View.Initialize(this, ref desc);
    }

    /// <summary>
    /// An additional, unique view of the resource, with a specific description.
    /// </summary>
    internal ViewDX12<VD> View { get; }
}
