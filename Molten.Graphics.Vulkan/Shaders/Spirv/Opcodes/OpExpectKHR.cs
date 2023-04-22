using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpExpectKHR : SpirvOpcode
    {
        public override uint ID => 5631;

        public uint ResultType => Ptr[1];

        public uint ResultID => Ptr[2];

        public uint Value => Ptr[3];

        public uint ExpectedValue => Ptr[4];
    }
}
