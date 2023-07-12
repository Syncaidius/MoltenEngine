using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal interface IRenderSurfaceVK : IRenderSurface
    {
        /// <summary>
        /// Gets surface clear color, if any.
        /// </summary>
        Color? ClearColor { get; set; }
    }
}
