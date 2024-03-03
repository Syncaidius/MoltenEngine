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
    public static GraphicsFormat ToSRVFormat(this DepthFormat format)
    {
        return format switch
        {
            DepthFormat.R32 => GraphicsFormat.R32_Float,
            DepthFormat.R16 => GraphicsFormat.R16_Float,
            DepthFormat.R32G8X24 => GraphicsFormat.R32_Float_X8X24_Typeless,
            _ => GraphicsFormat.R24_UNorm_X8_Typeless,
        };
    }

    public static GraphicsFormat ToGraphicsFormat(this DepthFormat format)
    {
        return format switch
        {
            DepthFormat.R32 => GraphicsFormat.R32_Typeless,
            DepthFormat.R16 => GraphicsFormat.R16_Typeless,
            DepthFormat.R32G8X24 => GraphicsFormat.R32G8X24_Typeless,
            _ => GraphicsFormat.R24G8_Typeless,
        };
    }

    public static GraphicsFormat ToDepthViewFormat(this DepthFormat format)
    {
        switch (format)
        {
            default:
            case DepthFormat.R24G8:
                return GraphicsFormat.D24_UNorm_S8_UInt;

            case DepthFormat.R32:
                return GraphicsFormat.D32_Float;

            case DepthFormat.R16:
                return GraphicsFormat.R16_Float;

            case DepthFormat.R32G8X24:
                return GraphicsFormat.R32_Float_X8X24_Typeless;
        }
    }

    public static bool HasStencil(this DepthFormat format)
    {
        return format == DepthFormat.R24G8 || format == DepthFormat.R32G8X24;
    }
}
