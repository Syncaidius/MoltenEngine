using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    public enum DepthStencilOperation
    {
        None = 0x0,

        Keep = 0x1,

        Zero = 0x2,

        Replace = 0x3,

        IncrSat = 0x4,

        DecrSat = 0x5,

        Invert = 0x6,

        Incr = 0x7,

        Decr = 0x8
    }
}
