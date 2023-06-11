using System;
using System.Collections.Generic;
using System.Linq;

namespace Molten.Graphics.Vulkan
{
    internal class DepthSurfaceVK : Texture2DVK, IDepthStencilSurface
    {
        internal DepthSurfaceVK(GraphicsDevice device, TextureDimensions dimensions, AntiAliasLevel aaLevel, 
            MSAAQuality sampleQuality, DepthFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, GraphicsTextureType.Surface2D, dimensions, aaLevel, sampleQuality, format.ToGraphicsFormat(), flags, allowMipMapGen, name)
        {
            DepthFormat = format;
            Viewport = new ViewportF(0, 0, Width, Height);
        }

        public void Clear(GraphicsPriority priority, DepthClearFlags flags, float depthValue = 1.0f, byte stencilValue = 0)
        {
            QueueTask(priority, new DepthClearTaskVK()
            {
                DepthValue = depthValue,
                StencilValue = stencilValue,
            });
        }

        public DepthFormat DepthFormat { get; }

        public ViewportF Viewport { get; }
    }
}
