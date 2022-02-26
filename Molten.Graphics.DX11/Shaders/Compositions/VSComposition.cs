using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class VSComposition : ShaderComposition<ID3D11VertexShader>
    {
        public VSComposition(HlslShader parentShader, bool optional) : 
            base(parentShader, optional, ShaderType.VertexShader)
        {
        }

        protected override unsafe ID3D11VertexShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11VertexShader* ppShader = null;
            Parent.Device.NativeDevice->CreateVertexShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
