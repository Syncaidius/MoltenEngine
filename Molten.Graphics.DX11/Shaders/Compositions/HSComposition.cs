using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class HSComposition : ShaderComposition<ID3D11HullShader>
    {
        public HSComposition(HlslShader parentShader) : 
            base(parentShader, ShaderType.Hull)
        {
        }

        protected override unsafe ID3D11HullShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11HullShader* ppShader = null;
            Parent.NativeDevice.Ptr->CreateHullShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
