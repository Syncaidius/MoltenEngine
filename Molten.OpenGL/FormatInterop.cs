using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    internal static class FormatInterop
    {

        internal static GraphicsFormat FromSizedInternal(this SizedInternalFormat sizedFormat)
        {
            switch (sizedFormat)
            {
                case SizedInternalFormat.R16: return GraphicsFormat.R16_Typeless;
                case SizedInternalFormat.R16f: return GraphicsFormat.R16_Float;
                case SizedInternalFormat.R16i: return GraphicsFormat.R16_SInt;
                case SizedInternalFormat.R16ui: return GraphicsFormat.R16_UInt;

                case SizedInternalFormat.R32f: return GraphicsFormat.R32_Float;
                case SizedInternalFormat.R32i: return GraphicsFormat.R32_SInt;
                case SizedInternalFormat.R32ui: return GraphicsFormat.R32_UInt;

                case SizedInternalFormat.R8: return GraphicsFormat.R8_Typeless;
                case SizedInternalFormat.R8i: return GraphicsFormat.R8_SInt;
                case SizedInternalFormat.R8ui: return GraphicsFormat.R8_UInt;

                case SizedInternalFormat.Rg16: return GraphicsFormat.R16G16_Typeless;
                case SizedInternalFormat.Rg16f: return GraphicsFormat.R16G16_Float;
                case SizedInternalFormat.Rg16i: return GraphicsFormat.R16G16_SInt;
                case SizedInternalFormat.Rg16ui: return GraphicsFormat.R16G16_UInt;

                case SizedInternalFormat.Rg32f: return GraphicsFormat.R32G32_Float;
                case SizedInternalFormat.Rg32i: return GraphicsFormat.R32G32_SInt;
                case SizedInternalFormat.Rg32ui: return GraphicsFormat.R32G32_UInt;

                case SizedInternalFormat.Rg8: return GraphicsFormat.R8G8_Typeless;
                case SizedInternalFormat.Rg8i: return GraphicsFormat.R8G8_SInt;
                case SizedInternalFormat.Rg8ui: return GraphicsFormat.R8G8_UInt;

                case SizedInternalFormat.Rgba16: return GraphicsFormat.R16G16B16A16_Typeless;
                case SizedInternalFormat.Rgba16f: return GraphicsFormat.R16G16B16A16_Float;
                case SizedInternalFormat.Rgba16i: return GraphicsFormat.R16G16B16A16_SInt;
                case SizedInternalFormat.Rgba16ui: return GraphicsFormat.R16G16B16A16_UInt;

                case SizedInternalFormat.Rgba32f: return GraphicsFormat.R32G32B32A32_Float;
                case SizedInternalFormat.Rgba32i: return GraphicsFormat.R32G32B32A32_SInt;
                case SizedInternalFormat.Rgba32ui: return GraphicsFormat.R32G32B32A32_UInt;

                case SizedInternalFormat.Rgba8: return GraphicsFormat.R8G8B8A8_Typeless;
                case SizedInternalFormat.Rgba8i: return GraphicsFormat.R8G8B8A8_SInt;
                case SizedInternalFormat.Rgba8ui: return GraphicsFormat.R8G8B8A8_UInt;

                default: throw new InternalFormatException(sizedFormat, "Unable to convert internal to common format.");
            }
        }

        internal static SizedInternalFormat ToSizedInternal(this GraphicsFormat format)
        {
            return (SizedInternalFormat)ToInternal(format);
        }

        internal static PixelInternalFormat ToPixelInternal(this GraphicsFormat format)
        {
            
            return (PixelInternalFormat)ToInternal(format);
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

                default: throw new GraphicsFormatException(format, "The format is an incompatible OpenGL -internal format.");
            }
        }
    }
}
