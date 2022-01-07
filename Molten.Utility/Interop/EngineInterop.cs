using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten
{
    /// <summary>
    /// Provides helper methods to aid interopability with native libraries or unsafe code.
    /// </summary>
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
                IntPtr ptr = handle.AddrOfPinnedObject();
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

        /// <summary>
        /// An implementation of <see cref="Array.Resize{T}(ref T[], int)"/> that is not constrained 
        /// to [<see cref="int.MaxValue"/>] number of elements.
        /// </summary>
        /// <param name="array">The array to be resized.</param>
        /// <param name="newSize">The new size of the array, must be at 
        /// least the same size as <paramref name="array"/></param>
        public unsafe static void ArrayResize<T>(ref T[] array, long newSize)
        {
            if (array.Length > newSize)
                throw new Exception("New array size cannot be smaller than the provided array's length.");

            Type t = typeof(T);
            uint eSize = (uint)Marshal.SizeOf(t);
            T[] newArray = new T[newSize];
            GCHandle hArray = GCHandle.Alloc(array, GCHandleType.Pinned);

            try
            {
                void* ptrArray = hArray.AddrOfPinnedObject().ToPointer();
                GCHandle hNewArray = GCHandle.Alloc(newArray, GCHandleType.Pinned);
                try
                {
                    ulong arrayBytes = (ulong)array.LongLength * eSize;
                    ulong available = (ulong)newArray.LongLength * eSize;
                    void* ptrNewArray = hNewArray.AddrOfPinnedObject().ToPointer();
                    Buffer.MemoryCopy(ptrArray, ptrNewArray, available, arrayBytes);
                }
                finally
                {
                    hNewArray.Free();
                }
            }
            finally
            {
                hArray.Free();
            }

            array = newArray;
        }
    }
}
