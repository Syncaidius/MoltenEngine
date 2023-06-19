using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal class RenderTargetView : ResourceViewDX11<ID3D11RenderTargetView1, RenderTargetViewDesc1>
    {
        internal RenderTargetView(GraphicsResource resource) :
            base(resource, GraphicsResourceFlags.None) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, RenderTargetViewDesc1* desc, ref ID3D11RenderTargetView1* view)
        {
            Device.Ptr->CreateRenderTargetView1(resource, desc, ref view);
        }
    }
}
