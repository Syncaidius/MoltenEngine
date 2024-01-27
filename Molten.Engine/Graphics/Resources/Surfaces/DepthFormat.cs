namespace Molten.Graphics;

public enum DepthFormat
{
    R24G8_Typeless = 0,

    R32_Typeless = 1,

    R16_Typeless = 2,

    R32G8X24_Typeless = 3,
}

public static class DepthFormatExtensions
{
    public static GraphicsFormat ToSRVFormat(this DepthFormat format)
    {
        return format switch
        {
            DepthFormat.R32_Typeless => GraphicsFormat.R32_Float,
            DepthFormat.R16_Typeless => GraphicsFormat.R16_Float,
            DepthFormat.R32G8X24_Typeless => GraphicsFormat.R32_Float_X8X24_Typeless,
            _ => GraphicsFormat.R24_UNorm_X8_Typeless,
        };
    }

    public static GraphicsFormat ToGraphicsFormat(this DepthFormat format)
    {
        return format switch
        {
            DepthFormat.R32_Typeless => GraphicsFormat.R32_Typeless,
            DepthFormat.R16_Typeless => GraphicsFormat.R16_Typeless,
            DepthFormat.R32G8X24_Typeless => GraphicsFormat.R32G8X24_Typeless,
            _ => GraphicsFormat.R24G8_Typeless,
        };
    }

    public static GraphicsFormat ToDepthViewFormat(this DepthFormat format)
    {
        switch (format)
        {
            default:
            case DepthFormat.R24G8_Typeless:
                return GraphicsFormat.D24_UNorm_S8_UInt;

            case DepthFormat.R32_Typeless:
                return GraphicsFormat.D32_Float;

            case DepthFormat.R16_Typeless:
                return GraphicsFormat.R16_Float;

            case DepthFormat.R32G8X24_Typeless:
                return GraphicsFormat.R32_Float_X8X24_Typeless;
        }
    }
}
