namespace Molten.Graphics.Textures.DDS;

[FlagsAttribute]
/// <summary>DDS_HEADER : dwFlags. http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982%28v=vs.85%29.aspx. 
/// Determines what details or data the dds header contains</summary>
internal enum DDSFlags : uint
{
    Capabilities = 0x1,

    Height = 0x2,

    Width = 0x4,

    Pitch = 0x8,

    PixelFormat = 0x1000,

    MipMapCount = 0x20000,

    LinearSize = 0x80000,

    Depth = 0x800000,
}
