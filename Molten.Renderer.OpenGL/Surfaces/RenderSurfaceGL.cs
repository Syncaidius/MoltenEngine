using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RenderSurfaceGL : Texture2DGL, IRenderSurface
    {
        public RenderSurfaceGL(Texture2DGL descTexture, TextureFlags flags) : base(descTexture, flags)
        {
        }

        internal RenderSurfaceGL(Texture2DGL descTexture) : base(descTexture)
        {
        }

        internal RenderSurfaceGL(
            RendererGL renderer, 
            int width, 
            int height, 
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm, 
            int mipCount = 1, 
            int arraySize = 1, 
            TextureFlags flags = TextureFlags.None, 
            int sampleCount = 1) : 
            base(renderer, width, height, format, mipCount, arraySize, flags, sampleCount)
        {
        }

        public Viewport Viewport => throw new NotImplementedException();

        public void Clear(Color color)
        {
            throw new NotImplementedException();
        }
    }
}
