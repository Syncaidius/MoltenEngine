using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// Generates a <see cref="ShaderReflection"/> object from a compiled SPIR-V shader, by parsing its bytecode.
    /// </summary>
    internal unsafe class SpirVReflector
    {
        uint* _instructions;
        ulong _numInstructions;

        internal SpirVReflector(void* byteCode, nuint numBytes)
        {
            if (numBytes % 4 != 0)
                throw new ArgumentException("Bytecode size must be a multiple of 4.", nameof(numBytes));

            _instructions = (uint*)byteCode;
            _numInstructions = numBytes / 4U;
        }
    }
}
