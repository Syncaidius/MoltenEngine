using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public enum SpirvExecutionModel
    {
        Vertex = 0,
        TessellationControl = 1,
        TessellationEvaluation = 2,
        Geometry = 3,
        Fragment = 4,
        GLCompute = 5,
        Kernel = 6,
        TaskNV = 5267,
        MeshNV = 5268,
        RayGenerationKHR = 5313,
        RayGenerationNV = 5313,
        IntersectionKHR = 5314,
        IntersectionNV = 5314,
        AnyHitKHR = 5315,
        AnyHitNV = 5315,
        ClosestHitKHR = 5316,
        ClosestHitNV = 5316,
        MissKHR = 5317,
        MissNV = 5317,
        CallableKHR = 5318,
        CallableNV = 5318,
        TaskEXT = 5364,
        MeshEXT = 5365,
        Max = 0x7fffffff,
    }
}
