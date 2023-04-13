using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct TextureDimensions
    {
        public uint Width = 1;

        public uint Height = 1;

        public uint Depth = 1;

        public uint MipMapLevels = 1;

        public uint ArraySize = 1;

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
