using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum GraphicsFormat
    {
        ///<Summary>The format is not known.</Summary>
        Unknown = 0,

        ///<Summary>A four-component, 128-bit typeless format that supports 32 bits per channel including alpha. 1</Summary>
        R32G32B32A32_Typeless = 1,

        ///<Summary>A four-component, 128-bit floating-point format that supports 32 bits per channel including alpha. 1</Summary>
        R32G32B32A32_Float = 2,

        ///<Summary>A four-component, 128-bit unsigned-integer format that supports 32 bits per channel including alpha. 1</Summary>
        R32G32B32A32_UInt = 3,

        ///<Summary>A four-component, 128-bit signed-integer format that supports 32 bits per channel including alpha. 1</Summary>
        R32G32B32A32_SInt = 4,

        ///<Summary>A three-component, 96-bit typeless format that supports 32 bits per color channel.</Summary>
        R32G32B32_Typeless = 5,

        ///<Summary>A three-component, 96-bit floating-point format that supports 32 bits per color channel.</Summary>
        R32G32B32_Float = 6,

        ///<Summary>A three-component, 96-bit unsigned-integer format that supports 32 bits per color channel.</Summary>
        R32G32B32_UInt = 7,

        ///<Summary>A three-component, 96-bit signed-integer format that supports 32 bits per color channel.</Summary>
        R32G32B32_SInt = 8,

        ///<Summary>A four-component, 64-bit typeless format that supports 16 bits per channel including alpha.</Summary>
        R16G16B16A16_Typeless = 9,

        ///<Summary>A four-component, 64-bit floating-point format that supports 16 bits per channel including alpha.</Summary>
        R16G16B16A16_Float = 10,

        ///<Summary>A four-component, 64-bit unsigned-normalized-integer format that 
        /// supports 16 bits per channel including alpha.</Summary>
        R16G16B16A16_UNorm = 11,

        ///<Summary>A four-component, 64-bit unsigned-integer format that supports
        ///  16 bits per channel including alpha.</Summary>
        R16G16B16A16_UInt = 12,

        ///<Summary>A four-component, 64-bit signed-normalized-integer format that supports 16 bits 
        /// per channel including alpha.</Summary>
        R16G16B16A16_SNorm = 13,

        ///<Summary>A four-component, 64-bit signed-integer format that 
        /// supports 16 bits per channel including alpha.</Summary>
        R16G16B16A16_SInt = 14,

        ///<Summary>A two-component, 64-bit typeless format that supports 32 bits for the red channel
        /// and 32 bits for the green channel.</Summary>
        R32G32_Typeless = 15,

        ///<Summary>A two-component, 64-bit floating-point format that supports 32 bits for the red
        /// channel and 32 bits for the green channel.</Summary>
        R32G32_Float = 16,

        ///<Summary>A two-component, 64-bit unsigned-integer format that supports 32 bits for the
        /// red channel and 32 bits for the green channel.</Summary>
        R32G32_UInt = 17,

        ///<Summary>A two-component, 64-bit signed-integer format that supports 32 bits for the red channel and 32 bits for the green channel.</Summary>
        R32G32_SInt = 18,

        ///<Summary>A two-component, 64-bit typeless format that supports 32 bits for the red channel,
        ///  8 bits for the green channel, and 24 bits are unused.</Summary>
        R32G8X24_Typeless = 19,

        ///<Summary>A 32-bit floating-point component, and two unsigned-integer components (with
        ///   an additional 32 bits). This format supports 32-bit depth, 8-bit stencil, and
        ///    24 bits are unused.</Summary>
        D32_Float_S8X24_UInt = 20,

        ///<Summary>
        ///  A 32-bit floating-point component, and two typeless components (with an additional
        ///  32 bits). This format supports 32-bit red channel, 8 bits are unused, and 24
        ///  bits are unused.</Summary>
        R32_Float_X8X24_Typeless = 21,

        ///<Summary>A 32-bit typeless component, and two unsigned-integer components (with an additional
        ///  32 bits). This format has 32 bits unused, 8 bits for green channel, and 24 bits
        ///  are unused.</Summary>
        X32_Typeless_G8X24_UInt = 22,

        ///<Summary>A four-component, 32-bit typeless format that supports 10 bits for each color and 2 bits for alpha.</Summary>
        R10G10B10A2_Typeless = 23,

        ///<Summary>A four-component, 32-bit unsigned-normalized-integer format that supports 10 bits for each color and 2 bits for alpha.</Summary>
        R10G10B10A2_UNorm = 24,

        ///<Summary>A four-component, 32-bit unsigned-integer format that supports 10 bits for each color and 2 bits for alpha.</Summary>
        R10G10B10A2_UInt = 25,

        ///<Summary>
        ///  Three partial-precision floating-point numbers encoded into a single 32-bit value
        ///  (a variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15)
        ///   exponent). There are no sign bits, and there is a 5-bit biased (15) exponent
        ///   for each channel, 6-bit mantissa for R and G, and a 5-bit mantissa for B, as
        ///   shown in the following illustration.</Summary>
        R11G11B10_Float = 26,

        ///<Summary>A four-component, 32-bit typeless format that supports 8 bits per channel including alpha.</Summary>
        R8G8B8A8_Typeless = 27,

        ///<Summary>A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits per channel including alpha.</Summary>
        R8G8B8A8_UNorm = 28,

        ///<Summary>A four-component, 32-bit unsigned-normalized integer sRGB format that supports 8 bits per channel including alpha.</Summary>
        R8G8B8A8_UNorm_SRgb = 29,

        ///<Summary>A four-component, 32-bit unsigned-integer format that supports 8 bits per channel including alpha.</Summary>
        R8G8B8A8_UInt = 30,

        ///<Summary>A four-component, 32-bit signed-normalized-integer format that supports 8 bits per channel including alpha.</Summary>
        R8G8B8A8_SNorm = 31,

        ///<Summary>A four-component, 32-bit signed-integer format that supports 8 bits per channel including alpha.</Summary>
        R8G8B8A8_SInt = 32,

        ///<Summary>A two-component, 32-bit typeless format that supports 16 bits for the red channel and 16 bits for the green channel.</Summary>
        R16G16_Typeless = 33,

        ///<Summary>A two-component, 32-bit floating-point format that supports 16 bits for the red channel and 16 bits for the green channel.</Summary>
        R16G16_Float = 34,

        ///<Summary>A two-component, 32-bit unsigned-normalized-integer format that supports 16 bits each for the green and red channels.</Summary>
        R16G16_UNorm = 35,

        ///<Summary>A two-component, 32-bit unsigned-integer format that supports 16 bits for the red channel and 16 bits for the green channel.</Summary>
        R16G16_UInt = 36,

        ///<Summary>A two-component, 32-bit signed-normalized-integer format that supports 16 bits for the red channel and 16 bits for the green channel.></Summary>
        R16G16_SNorm = 37,

        ///<Summary>A two-component, 32-bit signed-integer format that supports 16 bits for the red channel and 16 bits for the green channel.</Summary>
        R16G16_SInt = 38,

        ///<Summary>A single-component, 32-bit typeless format that supports 32 bits for the red channel.</Summary>
        R32_Typeless = 39,

        ///<Summary>A single-component, 32-bit floating-point format that supports 32 bits for depth.</Summary>
        D32_Float = 40,

        ///<Summary>A single-component, 32-bit floating-point format that supports 32 bits for the red channel.</Summary>
        R32_Float = 41,

        ///<Summary>A single-component, 32-bit unsigned-integer format that supports 32 bits for the red channel.</Summary>
        R32_UInt = 42,

        ///<Summary>A single-component, 32-bit signed-integer format that supports 32 bits for the red channel.</Summary>
        R32_SInt = 43,

        ///<Summary>A two-component, 32-bit typeless format that supports 24 bits for the red channel and 8 bits for the green channel.</Summary>
        R24G8_Typeless = 44,

        ///<Summary>A 32-bit z-buffer format that supports 24 bits for depth and 8 bits for stencil.</Summary>
        D24_UNorm_S8_UInt = 45,

        ///<Summary>
        ///  A 32-bit format, that contains a 24 bit, single-component, unsigned-normalized
        ///  integer, with an additional typeless 8 bits. This format has 24 bits red channel
        ///  and 8 bits unused.</Summary>
        R24_UNorm_X8_Typeless = 46,

        ///<Summary>
        ///  A 32-bit format, that contains a 24 bit, single-component, typeless format, with
        ///  an additional 8 bit unsigned integer component. This format has 24 bits unused
        ///  and 8 bits green channel.</Summary>
        X24_Typeless_G8_UInt = 47,

        ///<Summary>A two-component, 16-bit typeless format that supports 8 bits for the red channel and 8 bits for the green channel.</Summary>
        R8G8_Typeless = 48,

        ///<Summary>A two-component, 16-bit unsigned-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.</Summary>
        R8G8_UNorm = 49,

        ///<Summary>A two-component, 16-bit unsigned-integer format that supports 8 bits for the red channel and 8 bits for the green channel.</Summary>
        R8G8_UInt = 50,

        ///<Summary>A two-component, 16-bit signed-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.</Summary>
        R8G8_SNorm = 51,

        ///<Summary>A two-component, 16-bit signed-integer format that supports 8 bits for the red channel and 8 bits for the green channel.</Summary>
        R8G8_SInt = 52,

        ///<Summary>A single-component, 16-bit typeless format that supports 16 bits for the red channel.</Summary>
        R16_Typeless = 53,

        ///<Summary>A single-component, 16-bit floating-point format that supports 16 bits for the red channel.</Summary>
        R16_Float = 54,

        ///<Summary>A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for depth.</Summary>
        D16_UNorm = 55,

        ///<Summary>A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for the red channel.</Summary>
        R16_UNorm = 56,

        ///<Summary>A single-component, 16-bit unsigned-integer format that supports 16 bits for the red channel.</Summary>
        R16_UInt = 57,

        ///<Summary>A single-component, 16-bit signed-normalized-integer format that supports 16 bits for the red channel.</Summary>
        R16_SNorm = 58,

        ///<Summary>A single-component, 16-bit signed-integer format that supports 16 bits for the red channel.</Summary>
        R16_SInt = 59,

        ///<Summary>A single-component, 8-bit typeless format that supports 8 bits for the red channel.</Summary>
        R8_Typeless = 60,

        ///<Summary>A single-component, 8-bit unsigned-normalized-integer format that supports 8 bits for the red channel.</Summary>
        R8_UNorm = 61,

        ///<Summary>A single-component, 8-bit unsigned-integer format that supports 8 bits for the red channel.</Summary>
        R8_UInt = 62,

        ///<Summary>A single-component, 8-bit signed-normalized-integer format that supports 8 bits for the red channel.</Summary>
        R8_SNorm = 63,

        ///<Summary>A single-component, 8-bit signed-integer format that supports 8 bits for the red channel.</Summary>
        R8_SInt = 64,

        ///<Summary>A single-component, 8-bit unsigned-normalized-integer format for alpha only.</Summary>
        A8_UNorm = 65,

        ///<Summary>A single-component, 1-bit unsigned-normalized integer format that supports 1 bit for the red channel. 2.</Summary>
        R1_UNorm = 66,

        ///<Summary>
        ///  Three partial-precision floating-point numbers encoded into a single 32-bit value
        ///   all sharing the same 5-bit exponent (variant of s10e5, which is sign bit, 10-bit
        ///   mantissa, and 5-bit biased (15) exponent). There is no sign bit, and there is
        ///   a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel, as
        /// shown in the following illustration. 2.</Summary>
        R9G9B9E5_Sharedexp = 67,

        ///<Summary>
        ///  A four-component, 32-bit unsigned-normalized-integer format. This packed RGB
        ///  format is analogous to the UYVY format. Each 32-bit block describes a pair of
        ///  pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and
        ///  the G8 values are unique to each pixel. 3 Width must be even.</Summary>
        R8G8_B8G8_UNorm = 68,

        ///<Summary>A four-component, 32-bit unsigned-normalized-integer format. This packed RGB
        /// format is analogous to the YUY2 format. Each 32-bit block describes a pair of
        /// pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and
        /// the G8 values are unique to each pixel. 3 Width must be even.</Summary>
        G8R8_G8B8_UNorm = 69,

        ///<Summary>Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC1_Typeless = 70,

        ///<Summary>Four-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC1_UNorm = 71,

        ///<Summary>Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC1_UNorm_SRgb = 72,

        ///<Summary>Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC2_Typeless = 73,

        ///<Summary>Four-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC2_UNorm = 74,

        ///<Summary>Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC2_UNorm_SRgb = 75,

        ///<Summary>Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC3_Typeless = 76,

        ///<Summary>Four-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC3_UNorm = 77,

        ///<Summary>Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC3_UNorm_SRgb = 78,

        ///<Summary>One-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC4_Typeless = 79,

        ///<Summary>One-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC4_UNorm = 80,

        ///<Summary>One-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC4_SNorm = 81,

        ///<Summary>Two-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC5_Typeless = 82,

        ///<Summary>Two-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC5_UNorm = 83,

        ///<Summary>Two-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC5_SNorm = 84,

        ///<Summary>A three-component, 16-bit unsigned-normalized-integer format that supports 5 bits for blue, 6 bits for green, and 
        /// 5 bits for red. Direct3D 10 through Direct3D 11:??This value is defined for DXGI. However, Direct3D 10, 10.1, or 
        /// 11 devices do not support this format. Direct3D 11.1:??This value is not supported until Windows?8.</Summary> 
        B5G6R5_UNorm = 85,

        ///<Summary>A four-component, 16-bit unsigned-normalized-integer format that supports 5 bits for each color channel and 1-bit alpha. 
        /// Direct3D 10 through Direct3D 11:??This value is defined for DXGI. However, Direct3D 10, 10.1, or 11 devices 
        /// do not support this format. Direct3D 11.1:??This value is not supported until Windows?8.</Summary> 
        B5G5R5A1_UNorm = 86,

        ///<Summary>A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8-bit alpha.</Summary>
        B8G8R8A8_UNorm = 87,

        ///<Summary>A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8 bits unused.</Summary>
        B8G8R8X8_UNorm = 88,

        ///<Summary>A four-component, 32-bit 2.8-biased fixed-point format that supports 10 bits for each color channel and 2-bit alpha.</Summary>
        R10G10B10_Xr_Bias_A2_UNorm = 89,

        ///<Summary>A four-component, 32-bit typeless format that supports 8 bits for each channel including alpha. 4</Summary>
        B8G8R8A8_Typeless = 90,

        ///<Summary> A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each channel including alpha. 4</Summary>
        B8G8R8A8_UNorm_SRgb = 91,

        ///<Summary> A four-component, 32-bit typeless format that supports 8 bits for each color channel, and 8 bits are unused. 4</Summary>
        B8G8R8X8_Typeless = 92,

        ///<Summary>A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each color channel, and 8 bits are unused. 4.</Summary>
        B8G8R8X8_UNorm_SRgb = 93,

        ///<Summary>A typeless block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC6H_Typeless = 94,

        ///<Summary>A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>

        BC6H_Uf16 = 95,

        ///<Summary>A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC6H_Sf16 = 96,

        ///<Summary>A typeless block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary> 
        BC7_Typeless = 97,

        ///<Summary>A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC7_UNorm = 98,

        ///<Summary>A block-compression format. 4 For information about block-compression formats, see Texture Block Compression in Direct3D 11.</Summary>
        BC7_UNorm_SRgb = 99
    }
}
