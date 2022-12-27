using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class SRView : ResourceView<ID3D11ShaderResourceView, ShaderResourceViewDesc>
    {
        internal SRView(DeviceDX11 device) : base(device) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ref ShaderResourceViewDesc desc, ref ID3D11ShaderResourceView* view)
        {
            Device.NativeDevice->CreateShaderResourceView(resource, ref desc, ref view);
        }
    }
}
