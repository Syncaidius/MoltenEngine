using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Molten
{
    /// <summary>
    /// Provides storage for a struct which can also be used in byte-perfect comparisons or equality checks.
    /// </summary>
    public unsafe abstract class StructKey : IEquatable<StructKey>, IDisposable
    {
        protected ulong* _parts;
        protected uint _partCount;

        public override string ToString()
        {
            string s = "";
            for (uint i = 0; i < _partCount; i++)
                s += $"{(i > 0 ? "-" : "")}{_parts[i]:X}";

            return s;
        }

        public void Dispose()
        {
            EngineUtil.Free(ref _parts);
        }

        public bool Equals(StructKey other)
        {
            if (this != other)
            {
                if (_partCount != other._partCount)
                    return false;

                for (uint i = 0; i < _partCount; i++)
                {
                    if (_parts[i] != other._parts[i])
                        return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Generates an equatable key from a struct's memory layout.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class StructKey<T> : StructKey
        where T : unmanaged
    {

        /// <summary>
        /// Creates a new instance of <see cref="StructKey{T}"/> by duplicating a <paramref name="source"/> key.
        /// </summary>
        /// <param name="source"></param>
        public StructKey(StructKey<T> source)
        {
            _partCount = source._partCount;
            _parts = EngineUtil.AllocArray<ulong>(_partCount);

            int size = sizeof(T);
            Buffer.MemoryCopy(source._parts, _parts, size, size);
        }

        public StructKey()
        {
            T value = new T(); 
            Initialize(&value);
        }

        public StructKey(ref T value)
        {
            fixed (T* ptr = &value)
                Initialize(ptr);
        }

        public StructKey(T* value)
        {
            Initialize(value);
        }

        private void Initialize(T* value)
        {
            uint partSize = (uint)sizeof(ulong);
            uint size = (uint)sizeof(T);
            _partCount = size / partSize + (size % partSize > 0 ? 1U : 0U);
            _parts = EngineUtil.AllocArray<ulong>(_partCount);

            long destSize = _partCount * partSize; 
            Buffer.MemoryCopy(value, _parts, destSize, size);
        }

        public void Set(ref T value)
        {
            ((T*)_parts)[0] = value;
        }

        /// <summary>
        /// Sets the value represented by the key. This will also regenerate the key.
        /// </summary>
        /// <param name="value"></param>
        public void Set(T* value)
        {
            int size = sizeof(T);
            Buffer.MemoryCopy(value, _parts, size, size);
        }

        public Span<ulong> GetParts()
        {
            return new Span<ulong>(_parts, (int)_partCount);
        }

        public ref T Value
        {
            get => ref Unsafe.AsRef<T>(_parts);
        }

        public static implicit operator T(StructKey<T> key)
        {
            return *((T*)key._parts);
        }

        public static implicit operator T*(StructKey<T> key)
        {
            return (T*)key._parts;
        }
    }
}
