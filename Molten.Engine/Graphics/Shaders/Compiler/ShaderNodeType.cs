using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a supported shader header node
    /// </summary>
    public enum ShaderNodeType
    {
        Name = 0,

        Description = 1,

        Author = 3,

        Pass = 4,

        Version = 5,

        Iterations = 6,

        /// <summary>
        /// Vertex shader entry-point
        /// </summary>
        Vertex = 7,

        /// <summary>
        /// Geometry shader entry-point
        /// </summary>
        Geometry = 8,

        /// <summary>
        /// Hull shader entry-point
        /// </summary>
        Hull = 9,

        /// <summary>
        /// Domain shader entry-point
        /// </summary>
        Domain = 10,

        /// <summary>
        /// Pixel/fragment shader - entry-point
        /// </summary>
        Pixel = 11,

        /// <summary>
        /// Pixel/fragment shader - entry-point
        /// </summary>
        Fragment = Pixel,

        /// <summary>
        /// Depth-stencil state definition
        /// </summary>
        Depth = 12,

        /// <summary>
        /// Blend state definition
        /// </summary>
        Blend = 13,

        /// <summary>
        /// Rasterizer state definition
        /// </summary>
        Rasterizer = 14,
    }
}
