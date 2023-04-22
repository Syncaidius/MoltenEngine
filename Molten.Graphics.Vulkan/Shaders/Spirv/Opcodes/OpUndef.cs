using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpUndef : SpirvOpcode
    {
        public override uint ID => 1;

        public uint ResultType => Ptr[1];

        public uint ResultID => Ptr[2];
    }
}
