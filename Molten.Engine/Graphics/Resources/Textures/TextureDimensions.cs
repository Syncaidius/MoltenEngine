namespace Molten.Graphics;

public struct TextureDimensions
{
    public uint Width;

    public uint Height;

    public uint Depth;

    public uint MipMapCount;

    public uint ArraySize;

    public bool IsCubeMap;

    public AntiAliasLevel MultiSampleLevel;

    public MSAAQuality SampleQuality;

    public TextureDimensions()
    {
        Width = 1;
        Height = 1;
        Depth = 1;
        MipMapCount = 1;
        ArraySize = 1;
        IsCubeMap = false;
        MultiSampleLevel = AntiAliasLevel.None;
        SampleQuality = MSAAQuality.Default;
    }

    public TextureDimensions(uint width, uint height,
        AntiAliasLevel aaLevel = AntiAliasLevel.None, MSAAQuality aaQuality = MSAAQuality.Default, bool isCubeMap = false)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Depth = 1;
        MipMapCount = 1;
        ArraySize = 1;
        IsCubeMap = isCubeMap;
        MultiSampleLevel = AntiAliasLevel.None;
        SampleQuality = MSAAQuality.Default;
    }

    public TextureDimensions(uint width, uint height, uint depth, uint mipMapLevels, uint arraySize, 
        AntiAliasLevel aaLevel = AntiAliasLevel.None, MSAAQuality aaQuality = MSAAQuality.Default, bool isCubeMap = false)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Depth = Math.Max(1, depth);
        MipMapCount = Math.Max(1, mipMapLevels);
        ArraySize = Math.Max(1, arraySize);
        IsCubeMap = isCubeMap;
        MultiSampleLevel = aaLevel;
        SampleQuality = aaQuality;
    }

    public static bool operator ==(TextureDimensions a, TextureDimensions b)
    {
        return a.Width == b.Width &&
            a.Height == b.Height &&
            a.Depth == b.Depth &&
            a.ArraySize == b.ArraySize &&
            a.MipMapCount == b.MipMapCount;
    }

    public static bool operator !=(TextureDimensions a, TextureDimensions b)
    {
        return a.Width != b.Width ||
            a.Height != b.Height |
            a.Depth != b.Depth |
            a.ArraySize != b.ArraySize ||
            a.MipMapCount != b.MipMapCount;
    }
}
