namespace Molten.Graphics.Textures;

public class DDSSizeException : Exception
{
    public DDSSizeException(DDSFormat format, uint width, uint height) :
        base($"Block-compression requires the width and height of the texture to be a multiple of {BCHelper.BLOCK_DIMENSIONS}. Texture was {width}x{height}.")
    {
        Format = format;
        SourceWidth = width;
        SourceHeight = height;
    }

    public DDSFormat Format { get; }

    public uint SourceWidth { get; }

    public uint SourceHeight { get; }
}
