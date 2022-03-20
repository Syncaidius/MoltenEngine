using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct SurfaceClearChange : ITextureTask
    {
        public RenderSurface2D Surface;

        public Color Color;

        public bool Process(DeviceContext pipe, TextureBase texture)
        {
            Surface.Clear(pipe, Color);
            return false;
        }
    }
}
