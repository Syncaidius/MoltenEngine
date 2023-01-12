using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class DSComposition : ShaderComposition<ID3D11DomainShader>
    {
        public DSComposition(HlslShader parentShader) : 
            base(parentShader, ShaderType.Domain)
        {
        }

        protected override unsafe ID3D11DomainShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11DomainShader* ppShader = null;
            Parent.Device.Ptr->CreateDomainShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
