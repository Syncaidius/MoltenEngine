using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class SpirvInstruction
    {
        uint* _ptr;

        internal SpirvInstruction(uint* ptr)
        {
            _ptr = ptr;
            Words = new List<SpirvWord>();
        }

        public uint WordCount => _ptr[0] >> 16;

        public SpirvOpCode OpCode => (SpirvOpCode)(_ptr[0] & 0xFFFF);

        public List<SpirvWord> Words { get; }
    }
}
