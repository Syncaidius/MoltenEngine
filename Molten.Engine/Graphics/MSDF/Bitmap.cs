using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    internal unsafe class Bitmap<T>
        where T : unmanaged
    {
        T* pixels;
        int w, h;

        public Bitmap(int nPerPixel)
        {
            pixels = null;
            w = 0;
            h = 0;
            NPerPixel = nPerPixel;
        }

        public Bitmap(int nPerPixel, int width, int height)
        {
            NPerPixel = nPerPixel;
            w = width;
            h = height;
            pixels = EngineUtil.AllocArray<T>((nuint)(NPerPixel * w * h));
        }

        public Bitmap(BitmapRef<T> orig)
        {
            NPerPixel = orig.NPerPixel;
            Set(orig);
        }

        public Bitmap(Bitmap<T> orig)
        {
            NPerPixel = orig.NPerPixel;
            Set(orig);
        }

        ~Bitmap()
        {
            EngineUtil.Free(ref pixels);
        }
        public void Set(BitmapRef<T> orig)
        {
            Validation.NPerPixel(orig, NPerPixel);

            if (pixels != orig.pixels)
                EngineUtil.Free(ref pixels);

            w = orig.Width;
            h = orig.Height;
            pixels = EngineUtil.AllocArray<T>((nuint)(NPerPixel * w * h));
        }

        public void Set(Bitmap<T> orig)
        {
            Validation.NPerPixel(orig, NPerPixel);

            if (pixels != orig.pixels)
                EngineUtil.Free(ref pixels);

            w = orig.w;
            h = orig.h;
            pixels = EngineUtil.AllocArray<T>((nuint)(NPerPixel * w * h));
        }

        public int Width()
        {
            return w;
        }

        public int Height()
        {
            return h;
        }

        public static implicit operator BitmapRef<T>(Bitmap<T> b)
        {
            return new BitmapRef<T>(b.pixels, b.NPerPixel, b.w, b.h);
        }

        public T* Ptr => pixels;

        public T* this[int x, int y] => pixels + NPerPixel * (w * y + x);

        public int NPerPixel { get; private set; }
    }
}
