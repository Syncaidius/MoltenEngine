using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Vulkan
{
    public class SpirvString : SpirvLiteral
    {
        public string Value;

        public override unsafe uint Read(uint* ptrWord, uint wordCount)
        {
            return base.Read(ptrWord, wordCount);
        }
    }
}
