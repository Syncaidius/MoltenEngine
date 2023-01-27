using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class PSComposition : ShaderCompositionDX11<ID3D11PixelShader>
    {
        public PSComposition(HlslShader parentShader) : 
            base(parentShader, ShaderType.Pixel)
        {
        }

        protected override unsafe ID3D11PixelShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11PixelShader* ppShader = null;
            (Device as DeviceDX11).Ptr->CreatePixelShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
