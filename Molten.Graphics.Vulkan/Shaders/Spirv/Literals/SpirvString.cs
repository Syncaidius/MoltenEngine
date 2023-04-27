using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Vulkan
{
    public class SpirvString : SpirvLiteral
    {
        public string Value;

        public override unsafe uint Read(uint* ptrWord, uint remainingWords)
        {
            byte* ptr = (byte*)ptrWord;
            Value = Encoding.UTF8.GetString(ptr, (int)(remainingWords * sizeof(uint)));
            int nullIndex = Value.IndexOf('\0');

            if(nullIndex > -1)
                Value = Value.Substring(0, nullIndex);

            return remainingWords;
        }

        public override string ToString()
        {
            return $"'{Value}'";
        }
    }
}
