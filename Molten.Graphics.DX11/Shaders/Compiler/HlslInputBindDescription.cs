using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class HlslInputBindDescription
    {
        internal readonly ShaderInputBindDesc* Ptr;

        internal readonly string Name;

        internal HlslInputBindDescription(ShaderInputBindDesc* desc)
        {
            Ptr = desc;
            Name = SilkMarshal.PtrToString((nint)desc->Name);
        }
    }
}
