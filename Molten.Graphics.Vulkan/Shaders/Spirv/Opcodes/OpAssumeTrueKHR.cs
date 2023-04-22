using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpAssumeTrueKHR : SpirvOpcode
    {
        public override uint ID => 5630;

        public uint Condition => Ptr[1];
    }
}
