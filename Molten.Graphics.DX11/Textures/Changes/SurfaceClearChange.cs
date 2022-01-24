using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct SurfaceClearChange : ITextureTask
    {
        public RenderSurface Surface;

        public Color Color;
         
        public void Process(PipeDX11 pipe, TextureBase texture)
        {
            Surface.Clear(pipe, Color);
        }
    }
}
