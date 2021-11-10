using System.IO;

namespace Molten.Graphics.Textures
{
    internal class BC4UParser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC4_UNorm;

        internal override Color4[] Decode(BinaryReader imageReader, Logger log)
        {
            BC4_UNORM bc = new BC4_UNORM();
            bc.Read(imageReader);
            return BC4BC5.D3DXDecodeBC4U(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            BC4_UNORM bc = BC4BC5.D3DXEncodeBC4U(uncompressed);
            bc.Write(writer);
        }
    }

    internal class BC4SParser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC4_SNorm;

        internal override Color4[] Decode(BinaryReader imageReader, Logger log)
        {
            BC4_SNORM bc = new BC4_SNORM();
            bc.Read(imageReader);
            return BC4BC5.D3DXDecodeBC4S(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            BC4_SNORM bc = BC4BC5.D3DXEncodeBC4S(uncompressed);
            bc.Write(writer);
        }
    }
}
