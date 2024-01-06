namespace Molten.Graphics.Textures.DDS;

/// <summary>DDS_HEADER http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982%28v=vs.85%29.aspx. 
/// NOTE: Include flags in dwFlags for the members of the structure that contain valid data.</summary>
internal struct DDSHeader
{
    /// <summary>Size of structure. This member must be set to 124.</summary>
    public uint Size;

    /// <summary>Flags to indicate which members contain valid data. </summary>
    public DDSFlags Flags;

    /// <summary>Surface height (in pixels).</summary>
    public uint Height;

    /// <summary>Surface width (in pixels).</summary>
    public uint Width;

    /// <summary>The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a 
    /// compressed texture. For information about how to compute the pitch, see the DDS File Layout section of the Programming Guide for DDS.</summary>
    public uint PitchOrLinearSize;

    /// <summary>Depth of a volume texture (in pixels), otherwise unused. </summary>
    public uint Depth;

    /// <summary>Number of mipmap levels, otherwise unused.</summary>
    public uint MipMapCount;

    /// <summary>Unused.</summary>
    public uint[] Reserved;

    /// <summary>The pixel format (see DDS_PIXELFORMAT).</summary>
    public DDSPixelFormat PixelFormat;

    /// <summary>Specifies the complexity of the surfaces stored.
    /// Note  When you write .dds files, you should set the DDSCAPS_TEXTURE flag, and for multiple surfaces you should also set the DDSCAPS_COMPLEX flag. 
    /// However, when you read a .dds file, you should not rely on the DDSCAPS_TEXTURE and DDSCAPS_COMPLEX flags being 
    /// set because some writers of such a file might not set these flags.</summary>
    public DDSCapabilities Caps;

    /// <summary>Additional detail about the surfaces stored.</summary>
    public DDSAdditionalCapabilities Caps2;

    /// <summary>Unused.</summary>
    public uint Caps3;

    /// <summary>Unused.</summary>
    public uint Caps4;

    /// <summary>Unused.</summary>
    public uint Reserved2;
}
