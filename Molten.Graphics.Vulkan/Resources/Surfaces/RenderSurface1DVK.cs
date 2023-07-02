using System;
using System.Collections.Generic;
using System.Linq;

namespace Molten.Graphics.Vulkan
{
    public class RenderSurface1DVK : Texture1DVK, IRenderSurface1D
    {
        /// <summary>
        /// Creates a new instance of <see cref="RenderSurface2DVK"/>.
        /// </summary>
        /// <param name="device">The parent <see cref="GraphicsDevice"/>.</param>
        /// <param name="arraySize">The number of array slices (textures) within the texture array.</param>
        /// <param name="mipCount">The number of mip-map levels.</param>
        /// <param name="width">The width of the 1D texture.</param>
        /// <param name="format">The graphics format.</param>
        /// <param name="flags">Resource flags.</param>
        /// <param name="allowMipMapGen">If true, the generation of mip-maps will be allowed on the current <see cref="RenderSurface2DVK"/> instance.</param>
        /// <param name="name"></param>
        public RenderSurface1DVK(GraphicsDevice device, uint width, uint mipCount, uint arraySize, 
            GraphicsFormat format, GraphicsResourceFlags flags, 
            bool allowMipMapGen, string name) : 
            base(device, width, mipCount, arraySize, format, flags, allowMipMapGen, name, true)
        {
            Viewport = new ViewportF(0, 0, Width, 1);
        }

        /// <inheritdoc/>
        public void Clear(GraphicsPriority priority, Color color)
        {
            QueueTask(priority, new SurfaceClearTaskVK()
            {
                Color = color,
            });
        }

        /// <inheritdoc/>
        public ViewportF Viewport { get; protected set; }
    }
}
