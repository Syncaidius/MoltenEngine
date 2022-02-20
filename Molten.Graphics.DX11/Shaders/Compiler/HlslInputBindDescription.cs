using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class HlslInputBindDescription : IDisposable
    {
        internal ShaderInputBindDesc* Ptr;

        internal readonly string Name;

        internal HlslInputBindDescription(ID3D11ShaderReflection* reflection, uint rIndex)
        {
            Ptr = EngineUtil.Alloc<ShaderInputBindDesc>();
            reflection->GetResourceBindingDesc(rIndex, Ptr);
            Name = SilkMarshal.PtrToString((nint)Ptr->Name);
        }

        public void Dispose()
        {
            EngineUtil.Free(ref Ptr);
        }
    }
}
