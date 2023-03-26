using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe struct ResourceHandleVK
    {
        public void* Ptr;

        public DeviceMemory* Memory;
    }
}
