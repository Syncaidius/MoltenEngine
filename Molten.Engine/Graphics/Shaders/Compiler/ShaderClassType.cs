using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum ShaderClassType
    {
        /// <summary>
        /// A shader that is part of a material.
        /// </summary>
        Material = 0,

        /// <summary>
        /// A compute shader
        /// </summary>
        Compute = 1,
    }
}
