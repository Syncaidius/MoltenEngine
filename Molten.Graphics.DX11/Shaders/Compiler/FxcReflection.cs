using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class FxcReflection : IDisposable
    {
        internal ID3D11ShaderReflection* Ptr;

        internal FxcReflection(ID3D11ShaderReflection* reflection)
        {
            Ptr = reflection;
        }

        public void Dispose()
        {
            SilkUtil.ReleasePtr(ref Ptr);
        }
    }
}
