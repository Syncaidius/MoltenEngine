using System.Runtime.CompilerServices;

namespace Molten.Graphics
{
    public unsafe class ShaderClassResult
    {
        void* _byteCode;

        void* _debugData;

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
            return (T*)_byteCode;
        }

        public T* GetDebugDataAs<T>()
            where T : unmanaged
        {
            return (T*)_debugData;
        }

        public ShaderReflection Reflection { get; }

        public void* ByteCode => _byteCode;

        public void* DebugData => _debugData;

        public nuint NumBytes { get; }
    }
}
