namespace Molten.Graphics.Textures;

internal class BC1Parser : BCBlockParser
{
    public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC1_UNorm;

    internal override Color4[] Decode(BinaryReader imageReader, Logger log)
    {
        D3DX_BC1 bc1 = new D3DX_BC1();
        bc1.Read(imageReader);
        return BC.D3DXDecodeBC1(bc1);
    }

    internal override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
    {
        D3DX_BC1 bc1 = BC.D3DXEncodeBC1(uncompressed, 1.0f, BCFlags.DITHER_RGB);
        bc1.Write(writer);
    }
}
