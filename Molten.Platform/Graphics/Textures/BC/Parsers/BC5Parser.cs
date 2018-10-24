using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC5Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC5_UNorm };

        internal override Color4[] Decode(BinaryReader imageReader, BCDimensions dimensions, int levelWidth, int levelHeight)
        {
            BC5_UNORM bc = new BC5_UNORM();
            bc.Read(imageReader);
            return BC4BC5.D3DXDecodeBC5U(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, BCDimensions dimensions, TextureData.Slice level)
        {
            BC5_UNORM bc = BC4BC5.D3DXEncodeBC5U(uncompressed, BCFlags.NONE);
            bc.Write(writer);
        }
    }
}
