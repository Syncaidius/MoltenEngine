using System.IO;

namespace Molten.Graphics.Textures
{
    internal class BC5UParser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC5_UNorm;

        internal override Color4[] Decode(BinaryReader imageReader, Logger log)
        {
            BC5_UNORM bc = new BC5_UNORM();
            bc.Read(imageReader);
            return BC4BC5.D3DXDecodeBC5U(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            BC5_UNORM bc = BC4BC5.D3DXEncodeBC5U(uncompressed);
            bc.Write(writer);
        }
    }

    internal class BC5SParser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC5_SNorm;

        internal override Color4[] Decode(BinaryReader imageReader, Logger log)
        {
            BC5_SNORM bc = new BC5_SNORM();
            bc.Read(imageReader);
            return BC4BC5.D3DXDecodeBC5S(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            BC5_SNORM bc = BC4BC5.D3DXEncodeBC5S(uncompressed);
            bc.Write(writer);
        }
    }
}
