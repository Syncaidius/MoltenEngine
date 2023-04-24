using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public abstract class SpirvWord
    {
        public abstract unsafe void Read(uint* ptrWord, uint wordCount);
    }
}
