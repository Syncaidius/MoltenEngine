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
        DXT1 = GraphicsFormat.BC1_UNorm,
        BC1 = GraphicsFormat.BC1_UNorm,
        BC1_SRGB = GraphicsFormat.BC1_UNorm_SRgb,

        /// <summary>A format best suited to images with minimal coherent alpha channel data. Alpha channel is compressed to 4-bits per pixel.</summary>
        DXT3 = GraphicsFormat.BC2_UNorm,
        BC2 = GraphicsFormat.BC2_UNorm,
        BC2_SRGB = GraphicsFormat.BC2_UNorm_SRgb,

        /// <summary>A format best suited to images with highly coherent alpha channel data.</summary>
        DXT5 = GraphicsFormat.BC3_UNorm,
        BC3 = GraphicsFormat.BC3_UNorm,
        BC3_SRGB = GraphicsFormat.BC3_UNorm_SRgb,

        /// <summary>
        /// A format best suited to single-channel (i.e. only the red channel) images.
        /// </summary>
        BC4U = GraphicsFormat.BC4_UNorm,
        BC4S = GraphicsFormat.BC4_SNorm,

        /// <summary>
        /// A format best suited to two-channel images, such as encoded normal maps.
        /// </summary>
        BC5U = GraphicsFormat.BC5_UNorm,
        BC5S = GraphicsFormat.BC5_SNorm,

        BC6HU = GraphicsFormat.BC6H_Uf16,
        BC6HS = GraphicsFormat.BC6H_Sf16,

        BC7 = GraphicsFormat.BC7_UNorm,
        BC7_SRGB = GraphicsFormat.BC7_UNorm_SRgb,
    }

    public static class DDSFormatExtensions
    {
        public static GraphicsFormat ToGraphicsFormat(this DDSFormat format)
        {
            return (GraphicsFormat)format;
        }
    }
}
