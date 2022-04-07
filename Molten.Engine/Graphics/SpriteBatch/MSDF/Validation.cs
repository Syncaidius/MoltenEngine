using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    /// <summary>
    /// Internal class to aid validation of MSDF library.
    /// </summary>
    internal static class Validation
    {
        public static void NPerPixel<T>(BitmapRef<T> bitmap, int expectedN) where T : unmanaged
        {
            if(bitmap.NPerPixel != expectedN)
                throw new IndexOutOfRangeException($"A {nameof(BitmapRef<T>)} of {expectedN} component{(expectedN > 1 ? "s" : "")}-per-pixel is expected, not {bitmap.NPerPixel}.");
        }

        public static void NPerPixel<T>(BitmapConstRef<T> bitmap, int expectedN) where T : unmanaged
        {
            if (bitmap.NPerPixel != expectedN)
                throw new IndexOutOfRangeException($"A {nameof(BitmapConstRef<T>)} of {expectedN} component{(expectedN > 1 ? "s" : "")}-per-pixel is expected, not {bitmap.NPerPixel}.");
        }

        public static void NPerPixel<T>(Bitmap<T> bitmap, int expectedN) where T : unmanaged
        {
            if (bitmap.NPerPixel != expectedN)
                throw new IndexOutOfRangeException($"A {nameof(Bitmap<T>)} of {expectedN} component{(expectedN > 1 ? "s" : "")}-per-pixel is expected, not {bitmap.NPerPixel}.");
        }
    }
}
