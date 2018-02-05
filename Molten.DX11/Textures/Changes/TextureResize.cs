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

        public int NewArraySize;

        public void Process(GraphicsPipe pipe, TextureBase texture)
        {
            texture.SetSize(NewWidth, NewHeight, NewDepth, NewArraySize);
        }
    }
}
