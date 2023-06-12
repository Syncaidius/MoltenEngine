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

        public TextureDimensions()
        {
            Width = 1;
            Height = 1;
            Depth = 1;
            MipMapLevels = 1;
            ArraySize = 1;
        }

        public TextureDimensions(uint width, uint height)
        {
            Width = width;
            Height = height;
            Depth = 1;
            MipMapLevels = 1;
            ArraySize = 1;
        }

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
