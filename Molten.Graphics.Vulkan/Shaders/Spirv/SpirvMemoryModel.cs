using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public enum SpirvMemoryModel
    {
        Simple = 0,
        GLSL450 = 1,
        OpenCL = 2,
        Vulkan = 3,
        VulkanKHR = 3,
        Max = 0x7fffffff,
    }
}
