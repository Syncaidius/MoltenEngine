using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum GraphicsBufferFlags
    {
        None = 0,

        Vertex = 1,

        Index = 1 << 1,

        Structured = 1 << 2,

        UnorderedAccess = 1 << 3,

        ShaderResource = 1 << 4,
    }
}
