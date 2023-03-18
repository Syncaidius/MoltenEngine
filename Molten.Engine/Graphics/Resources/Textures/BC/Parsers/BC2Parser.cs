namespace Molten.Graphics.Textures
{
    internal class BC2Parser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC2_UNorm;

        internal override Color4[] Decode(BinaryReader imageReader, Logger log)
        {
            D3DX_BC2 bc = new D3DX_BC2();
            bc.Read(imageReader);
            return BC.D3DXDecodeBC2(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            D3DX_BC2 bc = BC.D3DXEncodeBC2(uncompressed, BCFlags.DITHER_RGB);
            bc.Write(writer);
        }
    }
}
