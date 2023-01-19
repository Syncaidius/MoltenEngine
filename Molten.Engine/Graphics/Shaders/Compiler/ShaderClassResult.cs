using System.Runtime.CompilerServices;

namespace Molten.Graphics
{
    public unsafe class ShaderClassResult
    {
        void* _byteCode;

        public unsafe ShaderClassResult(ShaderReflection reflection, void* byteCode, nuint numBytes)
        {
            Reflection = reflection;
            _byteCode = byteCode;
            NumBytes = numBytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetByteCodeAs<T>()
            where T : unmanaged
        {
            return (T*)ByteCode;
        }

        public ShaderReflection Reflection { get; }

        public void* ByteCode => _byteCode;

        public nuint NumBytes { get; }
    }
}
