using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC4Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC4_UNorm };

        internal override Color4[] Decode(BinaryReader imageReader, BCDimensions dimensions, int levelWidth, int levelHeight)
        {
            BC4_UNORM bc = new BC4_UNORM();
            bc.Read(imageReader);
            return BC4BC5.D3DXDecodeBC4U(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, BCDimensions dimensions, TextureData.Slice level)
        {
            BC4_UNORM bc = BC4BC5.D3DXEncodeBC4U(uncompressed, BCFlags.DITHER_RGB);
            bc.Write(writer);
        }
    }
}
