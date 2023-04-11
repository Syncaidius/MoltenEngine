using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct TextureDimensions
    {
        public uint Width;

        public uint Height;

        public uint Depth;

        public uint MipMapLevels;

        public uint ArraySize;

        public TextureDimensions(uint width, uint height, uint depth, uint mipMapLevels, uint arraySize)
        {
            Width = width;
            Height = height;
            Depth = depth;
            MipMapLevels = mipMapLevels;
            ArraySize = arraySize;
        }
    }
}
