namespace Molten.Graphics;

public enum DDSFormat
{
    /// <summary>A format based suited to images with little or no alpha channel. Alpha channel is compressed to 1-bit per pixel.
    /// This is more useful for things like leaf masks for trees, where the alpha is simply a solid color around main image.</summary>
    DXT1 = GpuResourceFormat.BC1_UNorm,
    BC1 = GpuResourceFormat.BC1_UNorm,
    BC1_SRGB = GpuResourceFormat.BC1_UNorm_SRgb,

    /// <summary>A format best suited to images with minimal coherent alpha channel data. Alpha channel is compressed to 4-bits per pixel.</summary>
    DXT3 = GpuResourceFormat.BC2_UNorm,
    BC2 = GpuResourceFormat.BC2_UNorm,
    BC2_SRGB = GpuResourceFormat.BC2_UNorm_SRgb,

    /// <summary>A format best suited to images with highly coherent alpha channel data.</summary>
    DXT5 = GpuResourceFormat.BC3_UNorm,
    BC3 = GpuResourceFormat.BC3_UNorm,
    BC3_SRGB = GpuResourceFormat.BC3_UNorm_SRgb,

    /// <summary>
    /// A format best suited to single-channel (i.e. only the red channel) images.
    /// </summary>
    BC4U = GpuResourceFormat.BC4_UNorm,
    BC4S = GpuResourceFormat.BC4_SNorm,

    /// <summary>
    /// A format best suited to two-channel images, such as encoded normal maps.
    /// </summary>
    BC5U = GpuResourceFormat.BC5_UNorm,
    BC5S = GpuResourceFormat.BC5_SNorm,

    BC6U = GpuResourceFormat.BC6H_Uf16,
    BC6HU = GpuResourceFormat.BC6H_Uf16,
    BC6S = GpuResourceFormat.BC6H_Sf16,
    BC6HS = GpuResourceFormat.BC6H_Sf16,

    BC7 = GpuResourceFormat.BC7_UNorm,
    BC7_SRGB = GpuResourceFormat.BC7_UNorm_SRgb,
}

public static class DDSFormatExtensions
{
    public static GpuResourceFormat ToGraphicsFormat(this DDSFormat format)
    {
        return (GpuResourceFormat)format;
    }
}
