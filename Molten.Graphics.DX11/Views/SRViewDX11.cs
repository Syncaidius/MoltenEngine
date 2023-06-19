using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal unsafe class SRViewDX11 : ResourceViewDX11<ID3D11ShaderResourceView1, ShaderResourceViewDesc1>
    {
        internal SRViewDX11(GraphicsResource resource) : 
            base(resource, GraphicsResourceFlags.None) { }

        protected override unsafe void OnCreateView(ID3D11Resource* resource, ShaderResourceViewDesc1* desc, ref ID3D11ShaderResourceView1* view)
        {
            Device.Ptr->CreateShaderResourceView1(resource, desc, ref view);
        }

        public static implicit operator ID3D11ShaderResourceView*(SRViewDX11 view)
        {
            return (ID3D11ShaderResourceView*)view.Ptr;
        }
    }
}
