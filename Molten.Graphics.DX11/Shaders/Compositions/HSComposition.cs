using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class HSComposition : ShaderComposition<ID3D11HullShader>
    {
        public HSComposition(HlslShader parentShader, bool optional) : 
            base(parentShader, optional, ShaderType.HullShader)
        {
        }

        protected override unsafe ID3D11HullShader* CreateShader(void* ptrBytecode, nuint numBytes)
        {
            ID3D11HullShader* ppShader = null;
            Parent.Device.NativeDevice->CreateHullShader(ptrBytecode, numBytes, null, &ppShader);
            return ppShader;
        }
    }
}
