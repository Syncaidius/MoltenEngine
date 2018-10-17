using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum DDSFormat
    {
        /// <summary>A format based suited to images with little or no alpha channel. Alpha channel is compressed to 1-bit per pixel.
        /// This is more useful for things like leaf masks for trees, where the alpha is simply a solid color around main image.</summary>
        DXT1 = 0,
        BC1 = 0,
        BC1_SRGB = 0,

        /// <summary>A format best suited to images with minimal coherent alpha channel data. Alpha channel is compressed to 4-bits per pixel.</summary>
        DXT3 = 2,

        /// <summary>A format best suited to images with highly coherent alpha channel data.</summary>
        DXT5 = 4,
    }

    public static class DDSFormatExtensions
    {
        public static GraphicsFormat ToGraphicsFormat(this DDSFormat format)
        {
            switch (format)
            {
                case DDSFormat.DXT1: return GraphicsFormat.BC1_UNorm;;
                case DDSFormat.DXT3: return GraphicsFormat.BC2_UNorm;;
                case DDSFormat.DXT5: return GraphicsFormat.BC3_UNorm;
                default: return GraphicsFormat.Unknown;
            }
        }
    }
}
