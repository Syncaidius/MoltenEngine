using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class HSComposition : ShaderCompositionDX11<ID3D11HullShader>
    {
        public HSComposition(HlslShader parentShader) : 
            base(parentShader, ShaderType.Hull)
        {
        }

        protected override unsafe ID3D11HullShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11HullShader* ppShader = null;
            (Device as DeviceDX11).Ptr->CreateHullShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
