using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class RenderTargetView : ResourceView<ID3D11RenderTargetView1, RenderTargetViewDesc1>
    {
        internal RenderTargetView(DeviceDX11 device) : base(device) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ref RenderTargetViewDesc1 desc, ref ID3D11RenderTargetView1* view)
        {
            Device.NativeDevice->CreateRenderTargetView1(resource, ref desc, ref view);
        }
    }
}
