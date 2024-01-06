using System.Runtime.CompilerServices;

namespace Molten.Graphics;

public unsafe class ShaderCodeResult : IDisposable
{
    void* _byteCode;

    void* _debugData;

    public unsafe ShaderCodeResult(ShaderReflection reflection, void* byteCode, nuint numBytes, void* debugData)
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

    public void Dispose()
    {
        Reflection?.Dispose();
        // TODO add disposal callback for correctly handling the release of ByteCode and DebugData.
    }

    public ShaderReflection Reflection { get; }

    public void* ByteCode => _byteCode;

    public void* DebugData => _debugData;

    public nuint NumBytes { get; }
}
