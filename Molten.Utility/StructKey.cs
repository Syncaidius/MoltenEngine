using System.Runtime.CompilerServices;

namespace Molten
{
    /// <summary>
    /// Provides storage for a struct which can also be used in byte-perfect comparisons or equality checks.
    /// </summary>
    public unsafe abstract class StructKey : IEquatable<StructKey>, IDisposable, ICloneable
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

        public abstract StructKey Clone();

        object ICloneable.Clone()
        {
            return Clone();
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

        /// <summary>
        /// Creates a new instance of <see cref="StructKey{T}"/> with the default value of <typeparamref name="T"/>.
        /// </summary>
        public StructKey()
        {
            T value = new T(); 
            Initialize(&value);
        }

        /// <summary>
        /// Creates a new instance of <see cref="StructKey{T}"/> using the provided <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to use for populating the struct key memory.</param>
        public StructKey(ref T value)
        {
            fixed (T* ptr = &value)
                Initialize(ptr);
        }

        /// <summary>
        /// Creates a new instance of <see cref="StructKey{T}"/> using the provided poitner <paramref name="value"/>.
        /// </summary>
        /// <param name="value">A pointer to the value to use for populating the struct key memory.</param>
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

        public override StructKey Clone()
        {
            return new StructKey<T>(this);
        }

        public Span<ulong> GetParts()
        {
            return new Span<ulong>(_parts, (int)_partCount);
        }

        public ref T Value => ref Unsafe.AsRef<T>(_parts);

        public T NonRefValue => Unsafe.AsRef<T>(_parts);

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
