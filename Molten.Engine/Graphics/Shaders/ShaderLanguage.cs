using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum ShaderLanguage
    {
        Unknown = 0,

        /// <summary>
        /// The shader language used by OpenGL
        /// </summary>
        Glsl = 1,

        /// <summary>
        /// The shader language used by DirectX.
        /// </summary>
        Hlsl = 2,

        /// <summary>The shader bytecode language used by Vulkan.</summary>
        SpirV = 3
    }
}
