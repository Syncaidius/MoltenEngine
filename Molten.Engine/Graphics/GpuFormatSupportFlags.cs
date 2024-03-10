namespace Molten.Graphics;

/// <summary>
/// Represents feature support flags for a <see cref="GpuResourceFormat"/>. Directly equivilent to the D3D11_FORMAT_SUPPORT or D3D12_FORMAT_SUPPORT1 enumerations.
/// </summary>
[Flags]
public enum GpuFormatSupportFlags : uint
{
    ///<Summary>The format is not known.</Summary>
    None = 0,

    ///<Summary>Supports buffer resources.</Summary>
    Buffer = 1,

    ///<Summary>Supports input-assembler vertex buffers.</Summary>
    IAVertexBuffer = 2,

    ///<Summary>Supports input-assembler index buffers.</Summary>
    IAIndexBuffer = 4,

    ///<Summary>Supports stream-output buffers.</Summary>
    SOBuffer = 8,

    ///<Summary>Supports 1D textures.</Summary>
    Texture1D = 0x10,

    ///<Summary>Supports 2D textures.</Summary>
    Texture2D = 0x20,

    ///<Summary>Supports 3D textures.</Summary>
    Texture3D = 0x40,

    ///<Summary>Supports cube textures.</Summary>
    Texturecube = 0x80,

    ///<Summary>Supports loading shaders.</Summary>
    ShaderLoad = 0x100,

    ///<Summary>Supports sampling shaders.</Summary>
    ShaderSample = 0x200,

    ///<Summary>Supports comparison sampling shaders.</Summary>
    ShaderSampleComparison = 0x400,

    ///<Summary>Supports monochrome text sampling shaders.</Summary>
    ShaderSampleMonoText = 0x800,

    ///<Summary>Supports mipmaps.</Summary>
    Mip = 0x1000,

    ///<Summary>Supports automatic generation of mipmaps.</Summary>
    MipAutogen = 0x2000,

    ///<Summary>Supports render targets.</Summary>
    RenderTarget = 0x4000,

    ///<Summary>Supports blending.</Summary>
    Blendable = 0x8000,

    ///<Summary>Supports depth-stencil buffers.</Summary>
    DepthStencil = 0x10000,

    ///<Summary>Supports CPU lockable resources.</Summary>
    CpuLockable = 0x20000,

    ///<Summary>Supports multisample resolve.</Summary>
    MultisampleResolve = 0x40000,

    ///<Summary>Supports display.</Summary>
    Display = 0x80000,

    ///<Summary>Supports casting within a bit layout.</Summary>
    CastWithinBitLayout = 0x100000,

    ///<Summary>Supports multisample render targets.</Summary>
    MultisampleRendertarget = 0x200000,

    ///<Summary>Supports loading multisample textures.</Summary>
    MultisampleLoad = 0x400000,

    ///<Summary>Supports shader gather operations.</Summary>
    ShaderGather = 0x800000,

    ///<Summary>Supports back buffer casting.</Summary>
    BackBufferCast = 0x1000000,

    ///<Summary>Supports typed unordered access views.</Summary>
    TypedUnorderedAccessView = 0x2000000,

    ///<Summary>Supports comparison sampling shaders with gather.</Summary>
    ShaderGatherComparison = 0x4000000,

    ///<Summary>Supports decoder output.</Summary>
    DecoderOutput = 0x8000000,

    ///<Summary>Supports video processor output.</Summary>
    VideoProcessorOutput = 0x10000000,

    ///<Summary>Supports video processor input.</Summary>
    VideoProcessorInput = 0x20000000,

    ///<Summary>Supports video encoder.</Summary>
    VideoEncoder = 0x40000000
}

