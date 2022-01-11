using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten
{
    /// <summary>
    /// Provides helper methods to aid interopability with native libraries or unsafe code.
    /// </summary>
    public unsafe static class EngineUtil
    {
        class AllocatedMemory
        {
            internal void* Ptr;
            internal nuint NumBytes;

            internal AllocatedMemory(nuint numBytes)
            {
                Ptr = NativeMemory.Alloc(numBytes);
                NumBytes = numBytes;
            }

            internal void Free()
            {
                NativeMemory.Free(Ptr);
            }
        }

        static ConcurrentDictionary<nuint, AllocatedMemory> _allocated;

        static EngineUtil()
        {
            _allocated = new ConcurrentDictionary<nuint, AllocatedMemory>();
        }

        public static void* Alloc(nuint numBytes)
        {
            AllocatedMemory mem = new AllocatedMemory(numBytes);
            _allocated.TryAdd((nuint)mem.Ptr, mem);
            return mem.Ptr;
        }

        public static T* Alloc<T>() where T : unmanaged
        {
            int sizeOf = Marshal.SizeOf<T>();
            void* ptr = Alloc((nuint)sizeOf);
            return (T*)ptr;
        }

        public static void Free<T>(ref T* ptr) where T : unmanaged
        {
            if (!_allocated.TryGetValue((nuint)ptr, out AllocatedMemory mem))
            {
                throw new Exception($"The pointer {(nuint)ptr} was not allocated by Molten's memory manager.");
            }
            else
            {
                mem.Free();
                ptr = null;
            }
        }

        public static nuint GetAllocSize(void* ptr)
        {
            if (!_allocated.TryGetValue((nuint)ptr, out AllocatedMemory mem))
                throw new Exception($"The pointer {(nuint)ptr} was not allocated by Molten's memory manager.");
            else
                return mem.NumBytes;
        }

        public static void FreeAll()
        {
            foreach (AllocatedMemory mem in _allocated.Values)
                mem.Free();

        }

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
        /// An implementation of <see cref="Array.Resize{T}(ref T[], int)"/> that is not constrained 
        /// to [<see cref="int.MaxValue"/>] number of elements.
        /// </summary>
        /// <param name="array">The array to be resized.</param>
        /// <param name="newSize">The new size of the array, must be at 
        /// least the same size as <paramref name="array"/></param>
        public static void ArrayResize<T>(ref T[] array, long newSize)
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
