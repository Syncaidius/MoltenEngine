using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public enum SpirvAccessQualifier
    {
        ReadOnly = 0,
        WriteOnly = 1,
        ReadWrite = 2,
        Max = 0x7fffffff,
    }
}
