using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC1Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC1_Typeless, GraphicsFormat.BC1_UNorm, GraphicsFormat.BC1_UNorm_SRgb };

        internal override Color4[] Decode(BinaryReader imageReader, BCDimensions dimensions, int levelWidth, int levelHeight)
        {
            D3DX_BC1 bc1 = new D3DX_BC1();
            bc1.Read(imageReader);
            return BC.D3DXDecodeBC1(bc1);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, BCDimensions dimensions, TextureData.Slice level)
        {
            D3DX_BC1 bc1 = BC.D3DXEncodeBC1(uncompressed, 1.0f, BCFlags.DITHER_RGB);
            bc1.Write(writer);
        }
    }
}
