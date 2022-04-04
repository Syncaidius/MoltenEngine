using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    internal class BitmapRef<T>
        where T : struct
    {
        public T[] Data;

        public BitmapRef()
        {
            Data = null;
            Width = 0;
            Height = 0;
        }

        public BitmapRef(T[] pixels, int width, int height)
        {
            Data = pixels;
            Width = width;
            Height = height;
        }

        public BitmapRef(int width, int height)
        {
            Data = new T[width * height];
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public ref T this [int x, int y] => ref Data[x + (Width * y)];
    }
}
