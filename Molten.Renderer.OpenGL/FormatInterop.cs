using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    internal static class FormatInterop
    {

        internal static GraphicsFormat FromInternal(this InternalFormat internalFormat)
        {
            switch (internalFormat)
            {
                case InternalFormat.R16: return GraphicsFormat.R16_Typeless;
                case InternalFormat.R16f: return GraphicsFormat.R16_Float;
                case InternalFormat.R16i: return GraphicsFormat.R16_SInt;
                case InternalFormat.R16ui: return GraphicsFormat.R16_UInt;

                case InternalFormat.R32f: return GraphicsFormat.R32_Float;
                case InternalFormat.R32i: return GraphicsFormat.R32_SInt;
                case InternalFormat.R32ui: return GraphicsFormat.R32_UInt;

                case InternalFormat.R8: return GraphicsFormat.R8_Typeless;
                case InternalFormat.R8i: return GraphicsFormat.R8_SInt;
                case InternalFormat.R8ui: return GraphicsFormat.R8_UInt;

                case InternalFormat.Rg16: return GraphicsFormat.R16G16_Typeless;
                case InternalFormat.Rg16f: return GraphicsFormat.R16G16_Float;
                case InternalFormat.Rg16i: return GraphicsFormat.R16G16_SInt;
                case InternalFormat.Rg16ui: return GraphicsFormat.R16G16_UInt;

                case InternalFormat.Rg32f: return GraphicsFormat.R32G32_Float;
                case InternalFormat.Rg32i: return GraphicsFormat.R32G32_SInt;
                case InternalFormat.Rg32ui: return GraphicsFormat.R32G32_UInt;

                case InternalFormat.Rg8: return GraphicsFormat.R8G8_Typeless;
                case InternalFormat.Rg8i: return GraphicsFormat.R8G8_SInt;
                case InternalFormat.Rg8ui: return GraphicsFormat.R8G8_UInt;

                case InternalFormat.Rgba16: return GraphicsFormat.R16G16B16A16_Typeless;
                case InternalFormat.Rgba16f: return GraphicsFormat.R16G16B16A16_Float;
                case InternalFormat.Rgba16i: return GraphicsFormat.R16G16B16A16_SInt;
                case InternalFormat.Rgba16ui: return GraphicsFormat.R16G16B16A16_UInt;

                case InternalFormat.Rgba32f: return GraphicsFormat.R32G32B32A32_Float;
                case InternalFormat.Rgba32i: return GraphicsFormat.R32G32B32A32_SInt;
                case InternalFormat.Rgba32ui: return GraphicsFormat.R32G32B32A32_UInt;

                case InternalFormat.Rgba8: return GraphicsFormat.R8G8B8A8_Typeless;
                case InternalFormat.Rgba8i: return GraphicsFormat.R8G8B8A8_SInt;
                case InternalFormat.Rgba8ui: return GraphicsFormat.R8G8B8A8_UInt;

                default: throw new GraphicsFormatException<InternalFormat>(internalFormat, "Unable to convert internal to common format.");
            }
        }

        internal static InternalFormat ToInternal(this GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.R16_Typeless: return InternalFormat.R16;
                case GraphicsFormat.R16_Float: return InternalFormat.R16f;
                case GraphicsFormat.R16_SInt: return InternalFormat.R16i;
                case GraphicsFormat.R16_UInt: return InternalFormat.R16ui;

                case GraphicsFormat.R32_Float: return InternalFormat.R32f;
                case GraphicsFormat.R32_SInt: return InternalFormat.R32i;
                case GraphicsFormat.R32_UInt: return InternalFormat.R32ui;

                case GraphicsFormat.R8_Typeless: return InternalFormat.R8;
                case GraphicsFormat.R8_SInt: return InternalFormat.R8i;
                case GraphicsFormat.R8_UInt: return InternalFormat.R8ui;

                case GraphicsFormat.R16G16_Typeless: return InternalFormat.Rg16;
                case GraphicsFormat.R16G16_Float: return InternalFormat.Rg16f;
                case GraphicsFormat.R16G16_SInt: return InternalFormat.Rg16i;
                case GraphicsFormat.R16G16_UInt: return InternalFormat.Rg16ui;

                case GraphicsFormat.R32G32_Float: return InternalFormat.Rg32f;
                case GraphicsFormat.R32G32_SInt: return InternalFormat.Rg32i;
                case GraphicsFormat.R32G32_UInt: return InternalFormat.Rg32ui;

                case GraphicsFormat.R8G8_Typeless: return InternalFormat.Rg8;
                case GraphicsFormat.R8G8_SInt: return InternalFormat.Rg8i;
                case GraphicsFormat.R8G8_UInt: return InternalFormat.Rg8ui;

                case GraphicsFormat.R16G16B16A16_Typeless: return InternalFormat.Rgba16;
                case GraphicsFormat.R16G16B16A16_Float: return InternalFormat.Rgba16f;
                case GraphicsFormat.R16G16B16A16_SInt: return InternalFormat.Rgba16i;
                case GraphicsFormat.R16G16B16A16_UInt: return InternalFormat.Rgba16ui;

                case GraphicsFormat.R32G32B32A32_Float: return InternalFormat.Rgba32f;
                case GraphicsFormat.R32G32B32A32_SInt: return InternalFormat.Rgba32i;
                case GraphicsFormat.R32G32B32A32_UInt: return InternalFormat.Rgba32ui;

                case GraphicsFormat.R8G8B8A8_Typeless: return InternalFormat.Rgba8;
                case GraphicsFormat.R8G8B8A8_SInt: return InternalFormat.Rgba8i;
                case GraphicsFormat.R8G8B8A8_UInt: return InternalFormat.Rgba8ui;

                default: throw new GraphicsFormatException<GraphicsFormat>(format, "The format is an incompatible OpenGL - internal format.");
            }
        }

        internal static GraphicsFormat FromPixelInternal(this PixelInternalFormat PixelInternalFormat)
        {
            switch (PixelInternalFormat)
            {
                case PixelInternalFormat.R16: return GraphicsFormat.R16_Typeless;
                case PixelInternalFormat.R16f: return GraphicsFormat.R16_Float;
                case PixelInternalFormat.R16i: return GraphicsFormat.R16_SInt;
                case PixelInternalFormat.R16ui: return GraphicsFormat.R16_UInt;

                case PixelInternalFormat.R32f: return GraphicsFormat.R32_Float;
                case PixelInternalFormat.R32i: return GraphicsFormat.R32_SInt;
                case PixelInternalFormat.R32ui: return GraphicsFormat.R32_UInt;

                case PixelInternalFormat.R8: return GraphicsFormat.R8_Typeless;
                case PixelInternalFormat.R8i: return GraphicsFormat.R8_SInt;
                case PixelInternalFormat.R8ui: return GraphicsFormat.R8_UInt;

                case PixelInternalFormat.Rg16: return GraphicsFormat.R16G16_Typeless;
                case PixelInternalFormat.Rg16f: return GraphicsFormat.R16G16_Float;
                case PixelInternalFormat.Rg16i: return GraphicsFormat.R16G16_SInt;
                case PixelInternalFormat.Rg16ui: return GraphicsFormat.R16G16_UInt;

                case PixelInternalFormat.Rg32f: return GraphicsFormat.R32G32_Float;
                case PixelInternalFormat.Rg32i: return GraphicsFormat.R32G32_SInt;
                case PixelInternalFormat.Rg32ui: return GraphicsFormat.R32G32_UInt;

                case PixelInternalFormat.Rg8: return GraphicsFormat.R8G8_Typeless;
                case PixelInternalFormat.Rg8i: return GraphicsFormat.R8G8_SInt;
                case PixelInternalFormat.Rg8ui: return GraphicsFormat.R8G8_UInt;

                case PixelInternalFormat.Rgba16: return GraphicsFormat.R16G16B16A16_Typeless;
                case PixelInternalFormat.Rgba16f: return GraphicsFormat.R16G16B16A16_Float;
                case PixelInternalFormat.Rgba16i: return GraphicsFormat.R16G16B16A16_SInt;
                case PixelInternalFormat.Rgba16ui: return GraphicsFormat.R16G16B16A16_UInt;

                case PixelInternalFormat.Rgba32f: return GraphicsFormat.R32G32B32A32_Float;
                case PixelInternalFormat.Rgba32i: return GraphicsFormat.R32G32B32A32_SInt;
                case PixelInternalFormat.Rgba32ui: return GraphicsFormat.R32G32B32A32_UInt;

                case PixelInternalFormat.Rgba8: return GraphicsFormat.R8G8B8A8_Typeless;
                case PixelInternalFormat.Rgba8i: return GraphicsFormat.R8G8B8A8_SInt;
                case PixelInternalFormat.Rgba8ui: return GraphicsFormat.R8G8B8A8_UInt;

                default: 
                    throw new GraphicsFormatException<PixelInternalFormat>(PixelInternalFormat, "Unable to convert internal to common format.");
            }
        }

        internal static PixelInternalFormat ToPixelInternal(this GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.R16_Typeless: return PixelInternalFormat.R16;
                case GraphicsFormat.R16_Float: return PixelInternalFormat.R16f;
                case GraphicsFormat.R16_SInt: return PixelInternalFormat.R16i;
                case GraphicsFormat.R16_UInt: return PixelInternalFormat.R16ui;

                case GraphicsFormat.R32_Float: return PixelInternalFormat.R32f;
                case GraphicsFormat.R32_SInt: return PixelInternalFormat.R32i;
                case GraphicsFormat.R32_UInt: return PixelInternalFormat.R32ui;

                case GraphicsFormat.R8_Typeless: return PixelInternalFormat.R8;
                case GraphicsFormat.R8_SInt: return PixelInternalFormat.R8i;
                case GraphicsFormat.R8_UInt: return PixelInternalFormat.R8ui;

                case GraphicsFormat.R16G16_Typeless: return PixelInternalFormat.Rg16;
                case GraphicsFormat.R16G16_Float: return PixelInternalFormat.Rg16f;
                case GraphicsFormat.R16G16_SInt: return PixelInternalFormat.Rg16i;
                case GraphicsFormat.R16G16_UInt: return PixelInternalFormat.Rg16ui;

                case GraphicsFormat.R32G32_Float: return PixelInternalFormat.Rg32f;
                case GraphicsFormat.R32G32_SInt: return PixelInternalFormat.Rg32i;
                case GraphicsFormat.R32G32_UInt: return PixelInternalFormat.Rg32ui;

                case GraphicsFormat.R8G8_Typeless: return PixelInternalFormat.Rg8;
                case GraphicsFormat.R8G8_SInt: return PixelInternalFormat.Rg8i;
                case GraphicsFormat.R8G8_UInt: return PixelInternalFormat.Rg8ui;

                case GraphicsFormat.R16G16B16A16_Typeless: return PixelInternalFormat.Rgba16;
                case GraphicsFormat.R16G16B16A16_Float: return PixelInternalFormat.Rgba16f;
                case GraphicsFormat.R16G16B16A16_SInt: return PixelInternalFormat.Rgba16i;
                case GraphicsFormat.R16G16B16A16_UInt: return PixelInternalFormat.Rgba16ui;

                case GraphicsFormat.R32G32B32A32_Float: return PixelInternalFormat.Rgba32f;
                case GraphicsFormat.R32G32B32A32_SInt: return PixelInternalFormat.Rgba32i;
                case GraphicsFormat.R32G32B32A32_UInt: return PixelInternalFormat.Rgba32ui;

                case GraphicsFormat.R8G8B8A8_Typeless: return PixelInternalFormat.Rgba8;
                case GraphicsFormat.R8G8B8A8_SInt: return PixelInternalFormat.Rgba8i;
                case GraphicsFormat.R8G8B8A8_UInt: return PixelInternalFormat.Rgba8ui;

                default: throw new GraphicsFormatException<GraphicsFormat>(format, "The format is an incompatible OpenGL - pixel internal format.");
            }
        }
    }
}
