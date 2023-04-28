using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class SpirvOperandKindDef
    {
        public string Category { get; set; }

        public string Kind { get; set; }

        public SpirvEnumerantDef[] Enumerants { get; set; } = new SpirvEnumerantDef[0];
    }
}
