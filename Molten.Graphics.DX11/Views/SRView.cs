using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class SRView : ResourceView<ID3D11ShaderResourceView1, ShaderResourceViewDesc1>
    {
        internal SRView(DeviceDX11 device) : base(device) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ref ShaderResourceViewDesc1 desc, ref ID3D11ShaderResourceView1* view)
        {
            Device.NativeDevice->CreateShaderResourceView1(resource, ref desc, ref view);
        }

        public static implicit operator ID3D11ShaderResourceView*(SRView view)
        {
            return (ID3D11ShaderResourceView*)view.Ptr;
        }
    }
}
