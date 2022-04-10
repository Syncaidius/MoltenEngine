using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
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

        public int Width { get; set; }

        public int Height { get; set; }

        /// <summary>
        /// Number of elements per pixel.
        /// </summary>
        public int NPerPixel { get; }

        public T* this [int x, int y] => pixels+NPerPixel * (Width * y + x);
    }
}
