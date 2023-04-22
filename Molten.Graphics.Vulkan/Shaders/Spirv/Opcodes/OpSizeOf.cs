using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpSizeOf : SpirvOpcode
    {
        public override uint ID => 321;

        public uint ResultType => Ptr[1];

        public uint ResultID => Ptr[2];

        public uint Pointer => Ptr[3];
    }
}
