using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class SpirvResultID : SpirvLiteral<uint>
    {
        public override string ToString()
        {
            return $"Result ({Value})";
        }
    }
}
