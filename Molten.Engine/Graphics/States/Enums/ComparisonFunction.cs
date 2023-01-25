using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    public enum ComparisonFunction
    {
        Never = 0x1,

        Less = 0x2,

        Equal = 0x3,

        LessEqual = 0x4,

        Greater = 0x5,

        NotEqual = 0x6,

        GreaterEqual = 0x7,

        Always = 0x8
    }
}
