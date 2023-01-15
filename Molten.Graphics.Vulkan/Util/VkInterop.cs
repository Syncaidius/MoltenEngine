using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal static class VkInterop
    {
        internal static GraphicsFormat FromApi(this Format format)
        {
            switch(format)
            {
                default:
                case Format.Undefined:
                    return GraphicsFormat.Unknown;

                // RGBA 8-bit
                case Format.R8G8B8A8SNorm: return GraphicsFormat.R8G8B8A8_SNorm;
                case Format.R8G8B8A8Unorm: return GraphicsFormat.R8G8B8A8_UNorm;
                case Format.R8G8B8A8Sint: return GraphicsFormat.R8G8B8A8_SInt;
                case Format.R8G8B8A8Srgb: return GraphicsFormat.R8G8B8A8_UNorm_SRgb;

                // BGRA 8-bit
                case Format.B8G8R8A8Sint: return GraphicsFormat.B8G8R8A8_Typeless;
                case Format.B8G8R8A8SNorm: return GraphicsFormat.B8G8R8A8_Typeless;
                case Format.B8G8R8A8Unorm: return GraphicsFormat.B8G8R8A8_UNorm;

                case Format.R32Sfloat: return GraphicsFormat.R32_Float;
                case Format.R32Sint: return GraphicsFormat.R32_SInt;
                case Format.R32Uint: return GraphicsFormat.R32_UInt;
            }
        }

        internal static Format ToApi(this GraphicsFormat format)
        {
            switch (format)
            {
                default:
                case GraphicsFormat.Unknown:
                    return Format.Undefined;

                // RGBA 8-bit
                case GraphicsFormat.R8G8B8A8_SNorm: return Format.R8G8B8A8SNorm;
                case GraphicsFormat.R8G8B8A8_UNorm: return Format.R8G8B8A8Unorm;
                case GraphicsFormat.R8G8B8A8_SInt: return Format.R8G8B8A8Sint;
                case GraphicsFormat.R8G8B8A8_UNorm_SRgb: return Format.R8G8B8A8Srgb;

                // BGRA 8-bit
                case GraphicsFormat.B8G8R8A8_Typeless: return Format.B8G8R8A8SNorm;
                case GraphicsFormat.B8G8R8A8_UNorm: return Format.B8G8R8A8Unorm;

                case GraphicsFormat.R32_Float: return Format.R32Sfloat;
                case GraphicsFormat.R32_SInt: return Format.R32Sint;
                case GraphicsFormat.R32_UInt: return Format.R32Uint;
            }
        }
    }
}
