using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC2Parser : BCBlockParser
    {
        public override GraphicsFormat[] SupportedFormats => new GraphicsFormat[] { GraphicsFormat.BC2_Typeless, GraphicsFormat.BC2_UNorm, GraphicsFormat.BC2_UNorm_SRgb };

        internal override Color4[] Decode(BinaryReader imageReader, BCDimensions dimensions, int levelWidth, int levelHeight)
        {
            D3DX_BC2 bc = new D3DX_BC2();
            bc.Read(imageReader);
            return BC.D3DXDecodeBC2(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed, BCDimensions dimensions, TextureData.Slice level)
        {
            D3DX_BC2 bc = BC.D3DXEncodeBC2(uncompressed, BCFlags.DITHER_RGB);
            bc.Write(writer);
        }
    }
}
