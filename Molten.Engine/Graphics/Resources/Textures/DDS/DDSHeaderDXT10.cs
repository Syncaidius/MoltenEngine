namespace Molten.Graphics.Textures.DDS;

internal struct DDSHeaderDXT10
{
    public GraphicsFormat ImageFormat;

    public DDSResourceDimension Dimension;

    public DDSMiscFlags MiscFlags;

    public uint ArraySize;

    public DDSMiscFlags2 MiscFlags2;
}

/// <summary>Identifies the type of DDS resource.</summary>
public enum DDSResourceDimension : uint
{
    /// <summary>Resource is a 1D texture. The dwWidth member of DDS_HEADER specifies the size of the texture. 
    /// Typically, you set the dwHeight member of DDS_HEADER to 1; you also must set the DDSD_HEIGHT flag 
    /// in the dwFlags member of DDS_HEADER.</summary>
    Texture1D = 2,

    /// <summary>Resource is a 2D texture with an area specified by the dwWidth and dwHeight members of DDS_HEADER. 
    /// You can also use this type to identify a cube-map texture. For more information about how to identify a cube-map texture, 
    /// see miscFlag and arraySize members.</summary>
    Texture2D = 3,

    /// <summary>Resource is a 3D texture with a volume specified by the dwWidth, dwHeight, 
    /// and dwDepth members of DDS_HEADER. You also must set the DDSD_DEPTH flag in the dwFlags member of DDS_HEADER.</summary>
    Texture3D = 4,
}

/// <summary>Identifies other, less common options for resources.</summary>
[FlagsAttribute]
public enum DDSMiscFlags : uint
{
    None = 0x0,

    /// <summary>Indicates a 2D texture is a cube-map texture.</summary>
    TextureCube = 0x4,
}

/// <summary>Contains additional metadata (formerly was reserved). The lower 3 bits indicate the alpha mode of the associated resource. 
/// The upper 29 bits are reserved and are typically 0. </summary>
public enum DDSMiscFlags2 : uint
{
    /// <summary>Alpha channel content is unknown. This is the value for legacy files, which typically is assumed to be 'straight' alpha.</summary>
    AlphaUnknown = 0x0,

    /// <summary>Any alpha channel content is presumed to use straight alpha.</summary>
    AlphaStraight = 0x1,

    /// <summary>Any alpha channel content is using premultiplied alpha. The only legacy file formats that indicate this information are 'DX2' and 'DX4'.</summary>
    AlphaPreMultiplied = 0x2,

    /// <summary>Any alpha channel content is all set to fully opaque.</summary>
    AlphaOpaque = 0x3,

    /// <summary>Any alpha channel content is being used as a 4th channel and is not intended to represent transparency (straight or premultiplied).</summary>
    AlphaCustom = 0x4,
}
