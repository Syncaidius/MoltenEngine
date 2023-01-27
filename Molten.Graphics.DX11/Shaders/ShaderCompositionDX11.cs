using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public abstract unsafe class ShaderCompositionDX11<T> : ShaderComposition
        where T : unmanaged
    {
        protected ShaderCompositionDX11(HlslShader parentShader, ShaderType type) :
            base(parentShader, type)
        { }

        protected override unsafe sealed void* OnBuildShader(void* byteCode)
        {
            ID3D10Blob* dx11ByteCode = (ID3D10Blob*)byteCode;
            void* ptrBytecode = dx11ByteCode->GetBufferPointer();
            nuint numBytes = dx11ByteCode->GetBufferSize();
            return CreateShader(ptrBytecode, numBytes);
        }

        protected override unsafe void ReleaseShaderPtr(ref void* ptr)
        {
            T* ptrBytecode = (T*)ptr;
            SilkUtil.ReleasePtr(ref ptrBytecode);
            ptr = null;
        }

        protected abstract T* CreateShader(void* ptrBytecode, nuint numBytes);
    }
}
