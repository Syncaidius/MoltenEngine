using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class DSComposition : ShaderComposition<ID3D11DomainShader>
    {
        public DSComposition(HlslShader parentShader, bool optional) : 
            base(parentShader, optional, ShaderType.DomainShader)
        {
        }

        protected override unsafe ID3D11DomainShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11DomainShader* ppShader = null;
            Parent.Device.NativeDevice->CreateDomainShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
