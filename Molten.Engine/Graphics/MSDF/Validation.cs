using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    /// <summary>
    /// Internal class to aid validation of MSDF library.
    /// </summary>
    internal static class Validation
    {
        public static void NPerPixel<T>(TextureData.SliceRef<T> bitmap, int expectedN) where T : unmanaged
        {
            if(bitmap.ElementsPerPixel != expectedN)
                throw new IndexOutOfRangeException($"A {nameof(TextureData.SliceRef<T>)} of {expectedN} component{(expectedN > 1 ? "s" : "")}-per-pixel is expected, not {bitmap.ElementsPerPixel}.");
        }
    }
}
