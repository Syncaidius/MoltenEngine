using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Molten.Graphics;

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

        public static void ResizeArray<T>(ref T* existing, nuint existingNumElements, nuint newNumElements) where T : unmanaged
        {
            nuint destBytes = (nuint)sizeof(T) * newNumElements;
            nuint existingBytes = (nuint)sizeof(T) * existingNumElements;
            T* newArray = (T*)Alloc(destBytes);

            Buffer.MemoryCopy(existing, newArray, destBytes, existingBytes);
            T* old = existing;
            existing = newArray;
            Free(ref old);
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
            if (ptr == null)
                return;

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

        public static void Free(ref void* ptr)
        {
            if (ptr == null)
                return;

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
            if (ptr == null)
                return;

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

        public static void MemSet(void* ptr, byte val, nuint numBytes)
        {
            // TODO optimize by using a larger type (ulong) and then setting the last 1 - 7 bytes (remainder) using byte.
            byte* p = (byte*)ptr;
            for (uint i = 0; i < numBytes; i++)
                p[i] = val;
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

        public static unsafe byte* StringToPtr(string str, Encoding encoding)
        {
            return StringToPtr(str, encoding, out ulong byteCount);
        }

        public static unsafe byte* StringToPtr(string str, Encoding encoding, out ulong byteCount)
        {
            byte[] bytes = encoding.GetBytes(str);
            byteCount = (ulong)bytes.LongLength;
            byte* ptrMem = (byte*)Alloc((nuint)bytes.Length);

            fixed (byte* ptrBytes = bytes)
                Buffer.MemoryCopy(ptrBytes, ptrMem, (nuint)bytes.Length, (nuint)bytes.Length);
            return ptrMem;
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

        /// <summary>
        /// Try parsing an enum value using Silk.NET's possible naming conventions. e.g. DepthWriteMask.DepthWriteMaskAll or ComparisonFunc.ComparisonLessEqual.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryParseEnum<T>(string strValue, out T value)
            where T : struct, IComparable
        {
            Type t = typeof(T);
            bool success = TryParseEnum(t, strValue, out object oValue);
            value = (T)oValue;
            return success;
        }

        /// <summary>
        /// Try parsing an enum value using Silk.NET's possible naming conventions. e.g. DepthWriteMask.DepthWriteMaskAll or ComparisonFunc.ComparisonLessEqual.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="strValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryParseEnum(Type t, string strValue, out object value)
        {
            strValue = strValue.ToLower();

            // First try to parse the value as-is.
            // If we fail, prefix it with the enum name to match Silk.NET's enum naming scheme. e.g. DepthWriteMask.DepthWriteMaskAll.
            if (!Enum.TryParse(t, strValue, true, out value))
            {
                // Now try adding the enum name into the value string.
                string strFullValue = $"{t.Name.ToLower()}{strValue}";

                if (!Enum.TryParse(t, strFullValue, true, out value))
                {
                    string[] split = Regex.Split(t.Name, "(?<!^)(?=[A-Z][\\d+]?)");
                    foreach (string s in split)
                        strValue = strValue.Replace(s, "");

                    // Try to parse by adding one word at a time, then two, etc.
                    for (int pCount = 1; pCount <= split.Length; pCount++)
                    {
                        string strNextValue = "";
                        for (int i = 0; i < pCount; i++)
                            strNextValue += split[i];

                        strNextValue += strValue;

                        if (Enum.TryParse(t, strNextValue, true, out value))
                            return true;
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts a PCI vendor ID to a <see cref="DeviceVendor"/>. Some vendors have multiple PCI IDs due to buyouts or mergers.
        /// </summary>
        /// <param name="pciID">The PCI ID of the vendor. </param>
        /// <returns></returns>
        public static DeviceVendor VendorFromPCI(uint pciID)
        {
            // See: https://pcisig.com/membership/member-companies
            // See: https://gamedev.stackexchange.com/a/31626/116135
            switch (pciID)
            {
                case 0x1002:
                case 0x1022:
                    return DeviceVendor.AMD;

                case 0x163C:
                case 0x8086:
                case 0x8087:
                    return DeviceVendor.Intel;

                case 0x10DE:
                    return DeviceVendor.Nvidia;

                default:
                    return DeviceVendor.Unknown;
            }
        }
    }
}
