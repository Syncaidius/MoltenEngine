using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal abstract class ShaderComposition : ContextBindable
    {
        /// <summary>A list of const buffers the shader stage requires to be bound.</summary>
        internal List<uint> ConstBufferIds = new List<uint>();

        /// <summary>A list of resources that must be bound to the shader stage.</summary>
        internal List<uint> ResourceIds = new List<uint>();

        /// <summary>A list of samplers that must be bound to the shader stage.</summary>
        internal List<uint> SamplerIds = new List<uint>();

        internal List<uint> UnorderedAccessIds = new List<uint>();

        internal ShaderIOStructure InputStructure;

        internal ShaderIOStructure OutputStructure;

        internal string EntryPoint;

        internal ShaderType Type;

        internal HlslShader Parent { get; }

        internal unsafe abstract void SetBytecode(ID3D10Blob* byteCode);

        internal ShaderComposition(HlslShader parentShader, ShaderType type) : 
            base(parentShader.NativeDevice, GraphicsBindTypeFlags.Input)
        {
            Parent = parentShader;
            Type = type;
        }

        protected override void OnApply(CommandQueueDX11 context) { }
    }

    internal abstract unsafe class ShaderComposition<T> : ShaderComposition 
        where T : unmanaged
    {
        T* _ptrShader;

        internal ShaderComposition(HlslShader parentShader, ShaderType type) : 
            base(parentShader, type) { }

        internal override unsafe void SetBytecode(ID3D10Blob* byteCode)
        {
            void* ptrBytecode = byteCode->GetBufferPointer();
            nuint numBytes = byteCode->GetBufferSize();
            _ptrShader = CreateShader(ptrBytecode, numBytes);
        }

        protected unsafe abstract T* CreateShader(void* ptrBytecode, nuint numBytes);

        /// <summary>The underlying, compiled HLSL shader object.</summary>
        internal T* PtrShader => _ptrShader;

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _ptrShader);
        }
    }
}
