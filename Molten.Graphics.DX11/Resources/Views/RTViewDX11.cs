using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

internal class RTViewDX11 : ResourceViewDX11<ID3D11RenderTargetView1, RenderTargetViewDesc1>
{
    internal RTViewDX11(ResourceHandleDX11 handle) :
        base(handle, GraphicsResourceFlags.None) { }

    protected override unsafe void OnCreateView(ID3D11Resource* resource, RenderTargetViewDesc1* desc, ref ID3D11RenderTargetView1* view)
    {
        Handle.Device.Ptr->CreateRenderTargetView1(resource, desc, ref view);
    }

    public static unsafe implicit operator ID3D11RenderTargetView*(RTViewDX11 view)
    {
        return (ID3D11RenderTargetView*)view.Ptr;
    }
}
