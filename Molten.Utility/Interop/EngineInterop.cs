using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

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

        /// <summary>
        /// Decodes a string from the provided byte array pointer, using the specified <see cref="Encoding"/>.
        /// If no <see cref="Encoding"/> is specified, the default one will be used.
        /// </summary>
        /// <param name="bytes">A pointer to an array of bytes.</param>
        /// <param name="encoding">An encoding to use. If null, the default <see cref="Encoding"/> will be used.</param>
        /// <returns></returns>
        public unsafe static string StringFromBytes(byte* bytes, Encoding encoding = null)
        {
            int len = 0;
            byte* p = bytes;
            byte c = *p;

            while (c != 0)
            {
                p++;
                len++;

                c = *p;
            }

            if (len > 0)
                return (encoding ?? Encoding.Default).GetString(bytes, len);
            else
                return string.Empty;
        }
    }
}
