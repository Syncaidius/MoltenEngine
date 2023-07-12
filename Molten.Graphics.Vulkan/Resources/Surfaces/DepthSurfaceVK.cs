using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class DepthSurfaceVK : Texture2DVK, IDepthStencilSurface
    {
        internal struct DepthClearValue
        {
            public float Depth;

            public uint Stencil;

            internal DepthClearValue(float depth, uint stencil)
            {
                Depth = depth;
                Stencil = stencil;
            }
        }

        internal DepthSurfaceVK(GraphicsDevice device, uint width, uint height, uint mipCount, uint arraySize,
            AntiAliasLevel aaLevel, 
            MSAAQuality sampleQuality, 
            DepthFormat format, 
            GraphicsResourceFlags flags, 
            bool allowMipMapGen, 
            string name) : 
            base(device, GraphicsTextureType.Surface2D, width, height, mipCount, arraySize, aaLevel, sampleQuality, format.ToGraphicsFormat(), flags, allowMipMapGen, name)
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

        /// <summary>
        /// Gets surface clear color, if any.
        /// </summary>
        internal DepthClearValue? ClearValue { get; set; }
    }
}
