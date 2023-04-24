using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum SpirvAddressingModel
    {
        Logical = 0,
        Physical32 = 1,
        Physical64 = 2,
        PhysicalStorageBuffer64 = 5348,
        PhysicalStorageBuffer64EXT = 5348,
    }
}
