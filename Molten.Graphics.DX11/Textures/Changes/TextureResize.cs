using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TextureResize : ITextureTask
    {
        public uint NewWidth;

        public uint NewHeight;

        public uint NewDepth;

        public uint NewMipMapCount;

        public uint NewArraySize;

        public Format NewFormat;

        public void Process(PipeDX11 pipe, TextureBase texture)
        {
            texture.SetSizeInternal(NewWidth, NewHeight, NewDepth, NewMipMapCount, NewArraySize, NewFormat);
        }
    }
}
