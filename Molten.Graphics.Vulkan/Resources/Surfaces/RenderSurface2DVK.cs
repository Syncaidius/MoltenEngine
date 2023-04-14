using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class RenderSurface2DVK : Texture2DVK, IRenderSurface2D
    {
        public RenderSurface2DVK(GraphicsDevice device, TextureDimensions dimensions, 
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, 
            GraphicsFormat format, GraphicsResourceFlags flags, 
            bool allowMipMapGen, string name) : 
            base(device, GraphicsTextureType.Surface2D, dimensions, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
            Viewport = new ViewportF(0, 0, Width, Height);
        }

        public ViewportF Viewport { get; protected set; }

        public void Clear(GraphicsPriority priority, Color color)
        {
            throw new NotImplementedException();
        }
    }
}
