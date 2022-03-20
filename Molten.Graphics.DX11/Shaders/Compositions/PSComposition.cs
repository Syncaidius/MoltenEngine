using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class PSComposition : ShaderComposition<ID3D11PixelShader>
    {
        public PSComposition(HlslShader parentShader, bool optional) : 
            base(parentShader, optional, ShaderType.PixelShader)
        {
        }

        protected override unsafe ID3D11PixelShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11PixelShader* ppShader = null;
            Parent.Device.NativeDevice->CreatePixelShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
