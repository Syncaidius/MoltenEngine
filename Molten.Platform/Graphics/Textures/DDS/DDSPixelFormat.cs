namespace Molten.Graphics.Textures.DDS
{
    /// <summary>DDS_PIXELFORMAT: http://msdn.microsoft.com/en-us/library/windows/desktop/bb943984%28v=vs.85%29.aspx. 
    /// Determines what pixel format data is contained in the DDS header.</summary>
    internal struct DDSPixelFormat
    {
        /// <summary>Structure size; set to 32 (bytes).</summary>
        public uint Size;

        /// <summary>Values which indicate what type of data is in the surface. </summary>
        public DDSPixelFormatFlags Flags;

        /// <summary>Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. 
        /// A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the 
        /// dxgiFormat member of that structure indicates the true format. When using a four-character code, dwFlags must include DDPF_FOURCC.</summary>
        public string FourCC;

        /// <summary>Number of bits in an RGB (possibly including alpha) format. Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.</summary>
        public uint RGBBitCount;

        /// <summary>Red (or lumiannce or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.</summary>
        public uint RBitMask;

        /// <summary>Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.</summary>
        public uint GBitMask;

        /// <summary>Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.</summary>
        public uint BBitMask;

        /// <summary>Alpha mask for reading alpha data. dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.</summary>
        public uint ABitMask;

        public bool HasFlags(DDSPixelFormatFlags flags)
        {
            return (Flags & flags) == flags;
        }
    }
}
