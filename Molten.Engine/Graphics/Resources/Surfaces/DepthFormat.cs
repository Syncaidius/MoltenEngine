namespace Molten.Graphics;

public enum DepthFormat : byte
{
    R24G8 = 0,

    R32 = 1,

    R16 = 2,

    R32G8X24 = 3,
}

public static class DepthFormatExtensions
{
    public static GpuResourceFormat ToSRVFormat(this DepthFormat format)
    {
        return format switch
        {
            DepthFormat.R32 => GpuResourceFormat.R32_Float,
            DepthFormat.R16 => GpuResourceFormat.R16_Float,
            DepthFormat.R32G8X24 => GpuResourceFormat.R32_Float_X8X24_Typeless,
            _ => GpuResourceFormat.R24_UNorm_X8_Typeless,
        };
    }

    public static GpuResourceFormat ToGraphicsFormat(this DepthFormat format)
    {
        return format switch
        {
            DepthFormat.R32 => GpuResourceFormat.R32_Typeless,
            DepthFormat.R16 => GpuResourceFormat.R16_Typeless,
            DepthFormat.R32G8X24 => GpuResourceFormat.R32G8X24_Typeless,
            _ => GpuResourceFormat.R24G8_Typeless,
        };
    }

    public static GpuResourceFormat ToDepthViewFormat(this DepthFormat format)
    {
        switch (format)
        {
            default:
            case DepthFormat.R24G8:
                return GpuResourceFormat.D24_UNorm_S8_UInt;

            case DepthFormat.R32:
                return GpuResourceFormat.D32_Float;

            case DepthFormat.R16:
                return GpuResourceFormat.R16_Float;

            case DepthFormat.R32G8X24:
                return GpuResourceFormat.R32_Float_X8X24_Typeless;
        }
    }

    public static bool HasStencil(this DepthFormat format)
    {
        return format == DepthFormat.R24G8 || format == DepthFormat.R32G8X24;
    }
}
