using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public abstract class SpirvWord
    {
        public virtual unsafe uint Read(uint* ptrWord, uint remainingWords)
        {
            return 1;
        }

        public string Name { get; internal set; }
    }
}
