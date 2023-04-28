using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Vulkan
{
    public class SpirvString : SpirvLiteral
    {
        public string Value;

        public override unsafe void Read(SpirvInstruction instruction)
        {
            byte* ptr = (byte*)instruction.ReadRemainingWords(out uint numWords);
            Value = Encoding.UTF8.GetString(ptr, (int)(numWords * sizeof(uint)));
            int nullIndex = Value.IndexOf('\0');

            if(nullIndex > -1)
                Value = Value.Substring(0, nullIndex);
        }

        public override string ToString()
        {
            return $"'{Value}'";
        }
    }
}
