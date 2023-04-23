using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class SpirvDecoration : SpirvLiteral
    {
        public override unsafe void Read(uint* ptrWord, uint wordCount)
        {
            DecorationType = (SpirvDecorationType)ptrWord[0];

            // TODO: Read decoration literals, if any.
            //       A JSON map will be needed to map decoration types to included literals (and tell us how many to read).
            //       See: https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Decoration
        }

        public SpirvDecorationType DecorationType { get; private set; }
    }
}
