using Molten.Collections;

namespace Molten.Graphics.Textures;

internal class BC7Parser : BCBlockParser
{
    public override GraphicsFormat ExpectedFormat => GraphicsFormat.BC7_UNorm;
    ObjectPool<BCContext> _contextPool = new ObjectPool<BCContext>(() => new BCContext());

    internal unsafe override Color4[] Decode(BinaryReader imageReader, Logger log)
    {
        D3DX_BC7 bc = new D3DX_BC7();
        bc.Read(imageReader);
        Color4[] colors = bc.Decode(log);
        Color4[] result = new Color4[colors.Length];

        int colSize = sizeof(Color4);
        fixed (Color4* ptrResult = result)
        {
            fixed (Color4* ptrColors = colors)
                Buffer.MemoryCopy(ptrColors, ptrResult, colSize * result.Length, colSize * colors.Length);
        }
        return result;
    }

    internal unsafe override void Encode(BinaryWriter writer, Color4[] uncompressed, Logger log)
    {
        D3DX_BC7 bc = new D3DX_BC7();
        BCContext context = _contextPool.GetInstance();
        bc.Encode(BCFlags.NONE, uncompressed, context);
        _contextPool.Recycle(context);
        bc.Write(writer);
    }
}
