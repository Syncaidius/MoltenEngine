using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    internal unsafe class BitmapConstRef<T>
        where T : unmanaged
    {
        public readonly T* pixels;

        public BitmapConstRef(int nPerPixel)
        {
            pixels = null;
            Width = 0;
            Height = 0;
            NPerPixel = nPerPixel;
        }

        public BitmapConstRef(T* pixels, int nPerPixel, int width, int height)
        {
            this.pixels = pixels;
            Width = width;
            Height = height;
            NPerPixel = nPerPixel;
        }

        public BitmapConstRef(BitmapRef<T> orig)
        {
            pixels = orig.pixels;
            Width = orig.Width;
            Height = orig.Height;
            NPerPixel = orig.NPerPixel;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        /// <summary>
        /// Number of elements per pixel.
        /// </summary>
        public int NPerPixel { get; }

        public T* this[int x, int y] => pixels + NPerPixel * (Width * y + x);
    }
}
