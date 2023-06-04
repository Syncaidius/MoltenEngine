namespace Molten.Graphics
{
    public enum DepthFormat
    {
        R24G8_Typeless = 0,

        R32_Typeless = 1,
    }

    public static class DepthFormatExtensions
    {
        public static GraphicsFormat ToGraphicsFormat(this DepthFormat format)
        {
            switch (format)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    return GraphicsFormat.R24G8_Typeless;

                case DepthFormat.R32_Typeless:
                    return GraphicsFormat.R32_Typeless;
            }
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
            }
        }
    }
}
