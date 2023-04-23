using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Vulkan
{
    public class SpirvString : SpirvLiteral
    {
        public string Value;

        public override unsafe void Read(uint* ptrWord, uint wordCount)
        {
            throw new NotImplementedException();
        }
    }
}
