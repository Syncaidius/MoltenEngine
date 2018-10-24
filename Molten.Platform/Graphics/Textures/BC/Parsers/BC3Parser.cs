using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC3Parser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC3_UNorm;

        internal override Color4[] Decode(BinaryReader imageReader)
        {
            D3DX_BC3 bc = new D3DX_BC3();
            bc.Read(imageReader);
            return BC.D3DXDecodeBC3(bc);
        }

        internal override void Encode(BinaryWriter writer, Color4[] uncompressed)
        {
            D3DX_BC3 bc = BC.D3DXEncodeBC3(uncompressed, BCFlags.DITHER_RGB);
            bc.Write(writer);
        }
    }
}
