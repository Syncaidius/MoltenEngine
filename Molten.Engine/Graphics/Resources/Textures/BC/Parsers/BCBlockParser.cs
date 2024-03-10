namespace Molten.Graphics.Textures;

/// <summary>A base class for DDS block readers.</summary>
internal abstract class BCBlockParser
{
    public abstract GpuResourceFormat ExpectedFormat { get; }

    internal abstract Color4[] Decode(BinaryReader imageReader, Logger log);

    internal abstract void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log);
}
