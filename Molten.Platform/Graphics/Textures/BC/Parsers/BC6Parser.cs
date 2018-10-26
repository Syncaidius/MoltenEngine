using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten.Graphics.Textures
{
    internal class BC6HUParser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC6H_Uf16;
        ObjectPool<D3DX_BC6H.Context> _contextPool = new ObjectPool<D3DX_BC6H.Context>(() => new D3DX_BC6H.Context());

        internal unsafe override Color4[] Decode(BinaryReader imageReader, Logger log)
        {
            D3DX_BC6H bc = new D3DX_BC6H();
            bc.Read(imageReader);
            HDRColorA[] colors = bc.Decode(false, log);
            Color4[] result = new Color4[colors.Length];

            int colSize = Marshal.SizeOf<Color4>();
            fixed (Color4* ptrResult = result)
            {
                fixed(HDRColorA* ptrColors = colors)
                    Buffer.MemoryCopy(ptrColors, ptrResult, colSize * result.Length, colSize * colors.Length);
            }
            return result;
        }

        internal unsafe override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            HDRColorA[] colors = new HDRColorA[uncompressed.Length];
            int colSize = Marshal.SizeOf<Color4>();
            fixed (Color4* ptrUncompressed = uncompressed)
            {
                fixed (HDRColorA* ptrColors = colors)
                    Buffer.MemoryCopy(ptrUncompressed, ptrColors, colSize * colors.Length, colSize * colors.Length);
            }

            D3DX_BC6H bc = new D3DX_BC6H();
            D3DX_BC6H.Context context = _contextPool.GetInstance();
            bc.Encode(false, colors, context);
            _contextPool.Recycle(context);
            bc.Write(writer);
        }
    }

    //internal class BC6HSParser : BCBlockParser
    //{
    //    public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC6H_Sf16;

    //    internal override Color4[] Decode(BinaryReader imageReader, Logger log)
    //    {
    //        BC5_SNORM bc = new BC5_SNORM();
    //        bc.Read(imageReader);
    //        return BC4BC5.D3DXDecodeBC5S(bc);
    //    }

    //    internal override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
    //    {
    //        BC5_SNORM bc = BC4BC5.D3DXEncodeBC5S(uncompressed);
    //        bc.Write(writer);
    //    }
    //}
}
