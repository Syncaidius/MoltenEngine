using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public abstract class SpirvLiteral : SpirvWord
    {
        
    }

    /// <summary>
    /// Reads a single word from the provided pointer as a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SpirvLiteral<T> : SpirvLiteral
        where T : unmanaged
    {
        public override unsafe void Read(uint* ptrWord, uint wordCount)
        {
            Value = *(T*)ptrWord;
        }

        public T Value;

    }
}
