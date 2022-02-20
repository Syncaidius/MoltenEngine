using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten
{
    /// <summary>
    /// Provides helper methods to aid interopability with native libraries or unsafe code.
    /// </summary>
    public unsafe static class EngineUtil
    {
        abstract class MemoryBase
        {
            internal void* Ptr;
            internal nuint NumBytes;

            internal MemoryBase(nuint numBytes)
            {
                NumBytes = numBytes;
                Alloc();
            }

            protected abstract void Alloc();

            internal abstract void Free();
        }

        class Memory : MemoryBase
        {
            internal Memory(nuint numBytes) : base(numBytes) { }

            protected override void Alloc()
            {
                Ptr = NativeMemory.Alloc(NumBytes);
            }

            internal override void Free()
            {
                NativeMemory.Free(Ptr);
            }
        }

        class AlignedMemory : MemoryBase
        {

            internal nuint Alignment;

            internal AlignedMemory(nuint numBytes, nuint alignment) : base(numBytes)
            {
                Alignment = alignment;
            }

            protected override void Alloc()
            {
                Ptr = NativeMemory.AlignedAlloc(NumBytes, Alignment);
            }

            internal override void Free()
            {
                NativeMemory.AlignedFree(Ptr);
            }
        }

        static ConcurrentDictionary<nuint, MemoryBase> _allocated;

        static EngineUtil()
        {
            _allocated = new ConcurrentDictionary<nuint, MemoryBase>();
        }

        public static void* Alloc(nuint numBytes)
        {
            Memory mem = new Memory(numBytes);
            _allocated.TryAdd((nuint)mem.Ptr, mem);
            return mem.Ptr;
        }

        public static void* AllocAligned(nuint numBytes, nuint alignment)
        {
            AlignedMemory mem = new AlignedMemory(numBytes, alignment);
            _allocated.TryAdd((nuint)mem.Ptr, mem);
            return mem.Ptr;
        }

        public static T* Alloc<T>() where T : unmanaged
        {
            return (T*)Alloc((nuint)sizeof(T));
        }

        public static T* AllocAligned<T>() where T : unmanaged
        {
            nuint sizeOf = (nuint)sizeof(T);
            return (T*)AllocAligned(sizeOf, sizeOf);
        }

        public static T* AllocAligned<T>(nuint alignment) where T : unmanaged
        {
            return (T*)AllocAligned((nuint)sizeof(T), alignment);
        }

        public static void* AllocArray(nuint elementSizeBytes, nuint numElements)
        {
            return Alloc(elementSizeBytes * numElements);
        }

        public static T* AllocArray<T>(nuint numElements) where T : unmanaged
        {
            return (T*)Alloc((nuint)sizeof(T) * numElements);
        }

        public static T* AllocAlignedArray<T>(nuint numElements, nuint alignment) where T : unmanaged
        {
            return (T*)AllocAligned((nuint)sizeof(T) * numElements, alignment);
        }

        public static T** AllocPtrArray<T>(nuint numElements) where T : unmanaged
        {
            return (T**)Alloc((uint)sizeof(T*) * numElements);
        }

        public static T** AllocAlignedPtrArray<T>(nuint numElements, nuint alignment) where T : unmanaged
        {
            return (T**)AllocAligned((uint)sizeof(T*) * numElements, alignment);
        }

        public static void Free<T>(ref T* ptr) where T : unmanaged
        {
            if (!_allocated.TryGetValue((nuint)ptr, out MemoryBase mem))
            {
                throw new Exception($"The pointer {(nuint)ptr} was not allocated by Molten's memory manager.");
            }
            else
            {
                mem.Free();
                ptr = null;
            }
        }

        public static void FreePtrArray<T>(ref T** ptr) where T : unmanaged
        {
            if (!_allocated.TryGetValue((nuint)ptr, out MemoryBase mem))
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
            if (!_allocated.TryGetValue((nuint)ptr, out MemoryBase mem))
                throw new Exception($"The pointer {(nuint)ptr} was not allocated by Molten's memory manager.");
            else
                return mem.NumBytes;
        }

        public static void FreeAll()
        {
            foreach (Memory mem in _allocated.Values)
                mem.Free();

        }

        /// <summary>A helper method for pinning a managed/C# object and providing an <see cref="IntPtr"/> to it. 
        /// Releases the pinned handle once finished.</summary>
        /// <param name="obj">The object.</param>
        /// <param name="callback">The callback.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PinObject(in object obj, Action<IntPtr> callback)
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
            if (array == null)
            {
                array = new T[newSize];
                return;
            }

            if (array.Length > newSize)
                throw new Exception("New array size cannot be smaller than the provided array's length.");

            Type t = typeof(T);
            T[] newArray = new T[newSize];

            if (t.IsValueType)
            {
                uint eSize = (uint)Marshal.SizeOf(t);
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
            }
            else
            {
                Array.Copy(array, newArray, array.LongLength);
            }

            array = newArray;
        }
    }
}
