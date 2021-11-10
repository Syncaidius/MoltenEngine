using System;

namespace Molten.Graphics.Textures
{
    public class DDSSizeException : Exception
    {
        public DDSSizeException(DDSFormat format, int width, int height) :
            base($"Block-compression requires the width and height of the texture to be a multiple of {BCHelper.BLOCK_DIMENSIONS}. Texture was {width}x{height}.")
        {
            Format = format;
            SourceWidth = width;
            SourceHeight = height;
        }

        public DDSFormat Format { get; }

        public int SourceWidth { get; }

        public int SourceHeight { get; }
    }
}
