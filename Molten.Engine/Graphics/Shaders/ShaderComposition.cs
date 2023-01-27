using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public unsafe abstract class ShaderComposition : GraphicsObject
    {
        /// <summary>A list of const buffers the shader stage requires to be bound.</summary>
        public List<uint> ConstBufferIds = new List<uint>();

        /// <summary>A list of resources that must be bound to the shader stage.</summary>
        public List<uint> ResourceIds = new List<uint>();

        /// <summary>A list of samplers that must be bound to the shader stage.</summary>
        public List<uint> SamplerIds = new List<uint>();

        public List<uint> UnorderedAccessIds = new List<uint>();

        public ShaderIOStructure InputStructure;

        public ShaderIOStructure OutputStructure;

        public string EntryPoint;

        public ShaderType Type;

        void* _ptrShader;

        public void BuildShader(void* byteCode)
        {
            _ptrShader = OnBuildShader(byteCode);
        }

        /// <summary>
        /// Invoked when shader bytecode is expected to be built into a shader object.
        /// </summary>
        /// <param name="byteCode">The shader bytecode to be built.</param>
        /// <returns>A pointer to the built shader object.</returns>
        protected abstract void* OnBuildShader(void* byteCode);

        protected ShaderComposition(HlslShader parentShader, ShaderType type) : 
            base(parentShader.Device, GraphicsBindTypeFlags.Input)
        {
            Parent = parentShader;
            Type = type;
        }

        protected override void OnApply(GraphicsCommandQueue context) { }

        public override sealed void GraphicsRelease()
        {
            ReleaseShaderPtr(ref _ptrShader);
        }

        protected abstract void ReleaseShaderPtr(ref void* ptr);

        public void* PtrShader => _ptrShader;

        public HlslShader Parent { get; }
    }
}
