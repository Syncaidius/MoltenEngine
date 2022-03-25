using Silk.NET.Core.Native;
using System.Text.RegularExpressions;

namespace Molten
{
    public unsafe static class SilkUtil
    {
        /// <summary>Releases the specified pointer, sets it to null and returns the updated, unmanaged reference count.</summary>
        /// <typeparam name="T">The type of pointer.</typeparam>
        /// <param name="ptr">The pointer.</param>
        /// <returns>The new pointer reference count.</returns>
        public static uint ReleasePtr<T>(ref T* ptr)
            where T : unmanaged
        {
            if (ptr == null)
                return 0;

            uint r = ((IUnknown*)ptr)->Release();
            ptr = null;
            return r;
        }
    }
}
