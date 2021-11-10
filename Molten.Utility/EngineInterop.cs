using System;
using System.Runtime.InteropServices;

namespace Molten
{
    public static class EngineInterop
    {
        /// <summary>A helper method for pinning a managed/C# object and providing an <see cref="IntPtr"/> to it. 
        /// Releases the pinned handle once finished.</summary>
        /// <param name="obj">The object.</param>
        /// <param name="callback">The callback.</param>
        public static void PinObject(object obj, Action<IntPtr> callback)
        {
            // Pin array so a pointer can be retrieved safely.
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);

            try
            {
                IntPtr ptr = (IntPtr)(handle.AddrOfPinnedObject().ToInt64());
                callback(ptr);
            }
            finally
            {
                handle.Free();
            }
        }

    }
}
