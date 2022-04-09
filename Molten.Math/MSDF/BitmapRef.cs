using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.MSDF
{
    public unsafe class BitmapRef<T>
        where T : unmanaged
    {
        public T* pixels;

        public BitmapRef(int nPerPixel)
        {
            pixels = null;
            Width = 0;
            Height = 0;
            NPerPixel = nPerPixel;
        }

        public BitmapRef(T* pixels, int nPerPixel, int width, int height)
        {
            this.pixels = pixels;
            Width = width;
            Height = height;
            NPerPixel = nPerPixel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="bitmap"></param>
        /// <param name="pos"></param>
        public static void Interpolate(float* output, BitmapRef<float> bitmap, Vector2D pos)
        {
            pos -= .5;
            int l = (int)Math.Floor(pos.X);
            int b = (int)Math.Floor(pos.Y);
            int r = l + 1;
            int t = b + 1;
            double lr = pos.X - l;
            double bt = pos.Y - b;
            l = (int)MsdfMath.clamp(l, bitmap.Width - 1); r = (int)MsdfMath.clamp(r, bitmap.Width - 1);
            b = (int)MsdfMath.clamp(b, bitmap.Height - 1); t = (int)MsdfMath.clamp(t, bitmap.Height - 1);
            for (int i = 0; i < bitmap.NPerPixel; ++i)
                output[i] = MsdfMath.mix(MsdfMath.mix(bitmap[l, b][i], bitmap[r, b][i], lr), MsdfMath.mix(bitmap[l, t][i], bitmap[r, t][i], lr), bt);
        }

        public int Width { get; set; }

        public int Height { get; set; }

        /// <summary>
        /// Number of elements per pixel.
        /// </summary>
        public int NPerPixel { get; }

        public T* this [int x, int y] => pixels+NPerPixel * (Width * y + x);
    }
}
