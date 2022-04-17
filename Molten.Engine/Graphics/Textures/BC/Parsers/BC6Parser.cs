using Molten.Collections;
using System.Runtime.InteropServices;

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
            Color4[] colors = bc.Decode(false, log);
            Color4[] result = new Color4[colors.Length];

            int colSize = Marshal.SizeOf<Color4>();
            fixed (Color4* ptrResult = result)
            {
                fixed (Color4* ptrColors = colors)
                    Buffer.MemoryCopy(ptrColors, ptrResult, colSize * result.Length, colSize * colors.Length);
            }
            return result;
        }

        internal unsafe override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            D3DX_BC6H bc = new D3DX_BC6H();
            D3DX_BC6H.Context context = _contextPool.GetInstance();
            bc.Encode(false, uncompressed, context);
            _contextPool.Recycle(context);
            bc.Write(writer);
        }
    }

    internal class BC6HSParser : BCBlockParser
    {
        public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC6H_Sf16;
        ObjectPool<D3DX_BC6H.Context> _contextPool = new ObjectPool<D3DX_BC6H.Context>(() => new D3DX_BC6H.Context());

        internal unsafe override Color4[] Decode(BinaryReader imageReader, Logger log)
        {
            D3DX_BC6H bc = new D3DX_BC6H();
            bc.Read(imageReader);
            Color4[] colors = bc.Decode(true, log);
            Color4[] result = new Color4[colors.Length];

            int colSize = Marshal.SizeOf<Color4>();
            fixed (Color4* ptrResult = result)
            {
                fixed (Color4* ptrColors = colors)
                    Buffer.MemoryCopy(ptrColors, ptrResult, colSize * result.Length, colSize * colors.Length);
            }
            return result;
        }

        internal unsafe override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
        {
            D3DX_BC6H bc = new D3DX_BC6H();
            D3DX_BC6H.Context context = _contextPool.GetInstance();
            bc.Encode(true, uncompressed, context);
            _contextPool.Recycle(context);
            bc.Write(writer);
        }
    }
}
