using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal unsafe class SRView : ResourceView<ID3D11ShaderResourceView1, ShaderResourceViewDesc1>
    {
        internal SRView(GraphicsResource resource) : 
            base(resource, GraphicsResourceFlags.None) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ShaderResourceViewDesc1* desc, ref ID3D11ShaderResourceView1* view)
        {
            Device.Ptr->CreateShaderResourceView1(resource, desc, ref view);
        }

        public static implicit operator ID3D11ShaderResourceView*(SRView view)
        {
            return (ID3D11ShaderResourceView*)view.Ptr;
        }
    }
}
