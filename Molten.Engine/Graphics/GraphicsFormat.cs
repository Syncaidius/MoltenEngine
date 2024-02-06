namespace Molten.Graphics;

public enum GraphicsFormat : byte
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
    BC7_UNorm_SRgb = 99,

    /// <summary>
    /// Most common YUV 4:4:4 video resource format. 
    /// Valid view formats for this video resource format are DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. 
    /// For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, 
    /// you can both read and write as opposed to just write for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. 
    /// Supported view types are SRV, RTV, and UAV.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    Ayuv = 100,

    /// <summary>
    /// 10-bit per channel packed YUV 4:4:4 video resource format. Valid view formats for this video resource format are 
    /// DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT. For UAVs, an additional valid view format is 
    /// DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for 
    /// DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT. Supported view types are SRV and UAV. 
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    Y410 = 101,

    /// <summary>16-bit per channel packed YUV 4:4:4 video resource format. 
    /// Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. 
    /// Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. 
    /// The mapping to the view channel is U->R16, Y->G16, V->B16 and A->A16.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format</summary>
    Y416 = 102,

    /// <summary>
    /// Most common YUV 4:2:0 video resource format. 
    /// Valid luminance data view formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT. 
    /// Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format 
    /// are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT. Supported view types are SRV, RTV, and UAV. 
    /// For luminance data view, the mapping to the view channel is Y->R8. For chrominance data view, the mapping to the 
    /// view channel is U->R8 and V->G8.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    NV12 = 103,

    /// <summary>
    /// 10-bit per channel planar YUV 4:2:0 video resource format. 
    /// Valid luminance data view formats for this video resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT. 
    /// The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 
    /// 10-bit format that uses 16 bits). If required, application shader code would have to enforce this manually. 
    /// From the runtime's point of view, DXGI_FORMAT_P010 is no different than DXGI_FORMAT_P016.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format 
    /// </summary>
    P010 = 104,

    /// <summary>
    /// 16-bit per channel planar YUV 4:2:0 video resource format. Valid luminance data view formats for this video 
    /// resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT. Valid chrominance data view formats 
    /// (width and height are each 1/2 of luminance view) for this video resource format are 
    /// DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. For UAVs, an additional valid chrominance data view 
    /// format is DXGI_FORMAT_R32_UINT. 
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    P016 = 105,

    /// <summary>
    /// 8-bit per channel planar YUV 4:2:0 video resource format. 
    /// This format is subsampled where each pixel has its own Y value, but each 2x2 pixel block shares a 
    /// single U and V value. The runtime requires that the width and height of all resources that are 
    /// created with this format are multiples of 2. The runtime also requires that the left, right, top, 
    /// and bottom members of any RECT that are used for this format are multiples of 2.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    Opaque_420 = 106,

    /// <summary>
    /// Most common YUV 4:2:2 video resource format. Valid view formats for this video resource format are
    /// DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT. 
    /// For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. 
    /// By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write 
    /// for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    Yuy2 = 107,

    /// <summary>
    /// 10-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource 
    /// format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. 
    /// The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 10-bit 
    /// format that uses 16 bits). If required, application shader code would have to enforce this manually. 
    /// From the runtime's point of view, DXGI_FORMAT_Y210 is no different than DXGI_FORMAT_Y216. 
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// This value is not supported until Windows 8.
    /// </summary>
    Y210 = 108,

    /// <summary>
    /// 16-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource format 
    /// are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT. Supported view types are SRV and UAV. 
    /// One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0->R16,
    /// U->G16, Y1->B16 and V->A16.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    Y216 = 109,

    /// <summary>
    /// Most common planar YUV 4:1:1 video resource format. Valid luminance data view 
    /// formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT. 
    /// Valid chrominance data view formats (width and height are each 1/4 of luminance view) for 
    /// this video resource format are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT. 
    /// Supported view types are SRV, RTV, and UAV. 
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    NV11 = 110,

    /// <summary>
    /// 4-bit palletized YUV format that is commonly used for DVD subpicture.
    /// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
    /// Direct3D 11.1:  This value is not supported until Windows 8.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    AI44 = 111,

    /// <summary>
    /// 4-bit palletized YUV format that is commonly used for DVD subpicture.
    /// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
    /// Direct3D 11.1:  This value is not supported until Windows 8.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    IA44 = 112,

    /// <summary>
    /// 8-bit palletized format that is used for palletized RGB data when the processor processes ISDB-T data and for palletized YUV data when the processor processes BluRay data.
    /// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
    /// Direct3D 11.1:  This value is not supported until Windows 8.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    P8 = 113,

    /// <summary>
    /// 8-bit palletized format with 8 bits of alpha that is used for palletized YUV data when the processor processes BluRay data.
    /// For more info about YUV formats for video rendering, see Recommended 8-Bit YUV Formats for Video Rendering.
    /// Direct3D 11.1:  This value is not supported until Windows 8.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    A8P8 = 114,

    /// <summary>
    /// A four-component, 16-bit unsigned-normalized integer format that supports 4 bits for each channel including alpha.
    /// Direct3D 11.1:  This value is not supported until Windows 8.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    B4G4R4A4_Unorm = 115,

    /// <summary>
    /// A video format; an 8-bit version of a hybrid planar 4:2:2 format.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    P208 = 130,

    /// <summary>
    /// An 8 bit YCbCrA 4:4 rendering format.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    V208 = 131,

    /// <summary>
    /// An 8 bit YCbCrA 4:4:4:4 rendering format.
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    V408 = 132,

    SamplerFeedbackMinMipOpaque = 189,

    SamplerFeedbackMipRegionUsedOpaque = 190,

    /// <summary>
    /// Forces this enumeration to compile to 32 bits in size. Without this value, some compilers would allow this enumeration to compile to a
    /// size other than 32 bits. This value is not used.
    /// </summary>
    ForceUInt = 191,
}

public static class GraphicsFormatExtensions
{
    public static DepthFormat ToDepthFormat(this GraphicsFormat format)
    {
        switch (format)
        {
            default:
            case GraphicsFormat.R24G8_Typeless:
            case GraphicsFormat.D24_UNorm_S8_UInt:
                return DepthFormat.R24G8;

            case GraphicsFormat.R32_Float:
            case GraphicsFormat.D32_Float:
                return DepthFormat.R32;
        }
    }

    public static uint BytesPerPixel(this GraphicsFormat format)
    {
        return BitsPerPixel(format) / 8;
    }

    public static uint BitsPerPixel(this GraphicsFormat format)
    {
        switch (format)
        {
            case GraphicsFormat.R32G32B32A32_Typeless:
            case GraphicsFormat.R32G32B32A32_Float:
            case GraphicsFormat.R32G32B32A32_UInt:
            case GraphicsFormat.R32G32B32A32_SInt:
                return 128;

            case GraphicsFormat.R32G32B32_Typeless:
            case GraphicsFormat.R32G32B32_Float:
            case GraphicsFormat.R32G32B32_UInt:
            case GraphicsFormat.R32G32B32_SInt:
                return 96;

            case GraphicsFormat.R16G16B16A16_Typeless:
            case GraphicsFormat.R16G16B16A16_Float:
            case GraphicsFormat.R16G16B16A16_UNorm:
            case GraphicsFormat.R16G16B16A16_UInt:
            case GraphicsFormat.R16G16B16A16_SNorm:
            case GraphicsFormat.R16G16B16A16_SInt:
            case GraphicsFormat.R32G32_Typeless:
            case GraphicsFormat.R32G32_Float:
            case GraphicsFormat.R32G32_UInt:
            case GraphicsFormat.R32G32_SInt:
            case GraphicsFormat.R32G8X24_Typeless:
            case GraphicsFormat.D32_Float_S8X24_UInt:
            case GraphicsFormat.R32_Float_X8X24_Typeless:
            case GraphicsFormat.X32_Typeless_G8X24_UInt:
            case GraphicsFormat.Y416:
            case GraphicsFormat.Y210:
            case GraphicsFormat.Y216:
                return 64;

            case GraphicsFormat.R10G10B10A2_Typeless:
            case GraphicsFormat.R10G10B10A2_UNorm:
            case GraphicsFormat.R10G10B10A2_UInt:
            case GraphicsFormat.R11G11B10_Float:
            case GraphicsFormat.R8G8B8A8_Typeless:
            case GraphicsFormat.R8G8B8A8_UNorm:
            case GraphicsFormat.R8G8B8A8_UNorm_SRgb:
            case GraphicsFormat.R8G8B8A8_UInt:
            case GraphicsFormat.R8G8B8A8_SNorm:
            case GraphicsFormat.R8G8B8A8_SInt:
            case GraphicsFormat.R16G16_Typeless:
            case GraphicsFormat.R16G16_Float:
            case GraphicsFormat.R16G16_UNorm:
            case GraphicsFormat.R16G16_UInt:
            case GraphicsFormat.R16G16_SNorm:
            case GraphicsFormat.R16G16_SInt:
            case GraphicsFormat.R32_Typeless:
            case GraphicsFormat.D32_Float:
            case GraphicsFormat.R32_Float:
            case GraphicsFormat.R32_UInt:
            case GraphicsFormat.R32_SInt:
            case GraphicsFormat.R24G8_Typeless:
            case GraphicsFormat.D24_UNorm_S8_UInt:
            case GraphicsFormat.R24_UNorm_X8_Typeless:
            case GraphicsFormat.X24_Typeless_G8_UInt:
            case GraphicsFormat.R9G9B9E5_Sharedexp:
            case GraphicsFormat.R8G8_B8G8_UNorm:
            case GraphicsFormat.G8R8_G8B8_UNorm:
            case GraphicsFormat.B8G8R8A8_UNorm:
            case GraphicsFormat.B8G8R8X8_UNorm:
            case GraphicsFormat.R10G10B10_Xr_Bias_A2_UNorm:
            case GraphicsFormat.B8G8R8A8_Typeless:
            case GraphicsFormat.B8G8R8A8_UNorm_SRgb:
            case GraphicsFormat.B8G8R8X8_Typeless:
            case GraphicsFormat.B8G8R8X8_UNorm_SRgb:
            case GraphicsFormat.Ayuv:
            case GraphicsFormat.Y410:
            case GraphicsFormat.Yuy2:
                return 32;

            case GraphicsFormat.P010:
            case GraphicsFormat.P016:
            case GraphicsFormat.V408:
                return 24;

            case GraphicsFormat.R8G8_Typeless:
            case GraphicsFormat.R8G8_UNorm:
            case GraphicsFormat.R8G8_UInt:
            case GraphicsFormat.R8G8_SNorm:
            case GraphicsFormat.R8G8_SInt:
            case GraphicsFormat.R16_Typeless:
            case GraphicsFormat.R16_Float:
            case GraphicsFormat.D16_UNorm:
            case GraphicsFormat.R16_UNorm:
            case GraphicsFormat.R16_UInt:
            case GraphicsFormat.R16_SNorm:
            case GraphicsFormat.R16_SInt:
            case GraphicsFormat.B5G6R5_UNorm:
            case GraphicsFormat.B5G5R5A1_UNorm:
            case GraphicsFormat.A8P8:
            case GraphicsFormat.B4G4R4A4_Unorm:
            case GraphicsFormat.P208:
            case GraphicsFormat.V208:
                return 16;

            case GraphicsFormat.NV12:
            case GraphicsFormat.Opaque_420:
            case GraphicsFormat.NV11:
                return 12;

            case GraphicsFormat.R8_Typeless:
            case GraphicsFormat.R8_UNorm:
            case GraphicsFormat.R8_UInt:
            case GraphicsFormat.R8_SNorm:
            case GraphicsFormat.R8_SInt:
            case GraphicsFormat.A8_UNorm:
            case GraphicsFormat.AI44:
            case GraphicsFormat.IA44:
            case GraphicsFormat.P8:
                return 8;

            case GraphicsFormat.BC2_Typeless:
            case GraphicsFormat.BC2_UNorm:
            case GraphicsFormat.BC2_UNorm_SRgb:
            case GraphicsFormat.BC3_Typeless:
            case GraphicsFormat.BC3_UNorm:
            case GraphicsFormat.BC3_UNorm_SRgb:
            case GraphicsFormat.BC5_Typeless:
            case GraphicsFormat.BC5_UNorm:
            case GraphicsFormat.BC5_SNorm:
            case GraphicsFormat.BC6H_Typeless:
            case GraphicsFormat.BC6H_Uf16:
            case GraphicsFormat.BC6H_Sf16:
            case GraphicsFormat.BC7_Typeless:
            case GraphicsFormat.BC7_UNorm:
            case GraphicsFormat.BC7_UNorm_SRgb:
                return 8;

            case GraphicsFormat.BC1_Typeless:
            case GraphicsFormat.BC1_UNorm:
            case GraphicsFormat.BC1_UNorm_SRgb:
            case GraphicsFormat.BC4_Typeless:
            case GraphicsFormat.BC4_UNorm:
            case GraphicsFormat.BC4_SNorm:
                return 4;

            case GraphicsFormat.R1_UNorm:
                return 1;
        }

        throw new GraphicsFormatException(GraphicsFormat.Unknown, $"Failed to get bits-per-pixel for GraphicsFormat.{format}");
    }
}
