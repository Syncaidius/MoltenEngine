using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TextureResize : ITextureChange
    {
        public int NewWidth;

        public int NewHeight;

        public int NewDepth;

        public int NewMipMapCount;

        public int NewArraySize;

        public Format NewFormat;

        public void Process(PipeDX11 pipe, TextureBase texture)
        {
            texture.SetSizeInternal(NewWidth, NewHeight, NewDepth, NewMipMapCount, NewArraySize, NewFormat);
        }
    }
}
