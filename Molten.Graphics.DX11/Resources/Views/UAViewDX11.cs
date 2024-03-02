using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

internal unsafe class UAViewDX11 : ViewDX11<ID3D11UnorderedAccessView1, UnorderedAccessViewDesc1>
{
    internal UAViewDX11(ResourceHandleDX11 handle) : 
        base(handle, GraphicsResourceFlags.UnorderedAccess) { }

    protected override unsafe void OnCreateView(ID3D11Resource* resource, UnorderedAccessViewDesc1* desc, ref ID3D11UnorderedAccessView1* view)
    {
        Handle.Device.Handle->CreateUnorderedAccessView1(resource, desc, ref view);
    }

    public static implicit operator ID3D11UnorderedAccessView*(UAViewDX11 view)
    {
        return (ID3D11UnorderedAccessView*)view.Ptr;
    }
}
