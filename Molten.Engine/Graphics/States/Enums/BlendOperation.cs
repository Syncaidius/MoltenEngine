using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    public enum BlendOperation
    {
        Invalid = 0,

        Add = 0x1,

        Subtract = 0x2,

        RevSubtract = 0x3,

        Min = 0x4,

        Max = 0x5
    }
}
