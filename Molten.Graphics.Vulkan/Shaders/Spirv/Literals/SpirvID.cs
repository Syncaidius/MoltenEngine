using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class SpirvID : SpirvLiteral<uint>
    {

        public override string ToString()
        {
            return $"ID ({Value})";
        }
    }
}
