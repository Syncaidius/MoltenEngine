using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe class ShaderIOStructureDX11 : ShaderIOStructure
    {
        internal InputElementDesc[] Elements { get; private set; }

        public ShaderIOStructureDX11(uint elementCount) : base(elementCount) { }

        public ShaderIOStructureDX11(ShaderCodeResult result, ShaderIOStructureType type) : 
            base(result, type) { }

        protected override void Initialize(uint elementCount)
        {
            Elements = new InputElementDesc[elementCount];
        }

        protected override void BuildElement(ShaderCodeResult result, ShaderIOStructureType type, ShaderParameterInfo pInfo, GraphicsFormat format, int index)
        {
            Elements[index] = new InputElementDesc()
            {
                SemanticName = (byte*)pInfo.SemanticNamePtr,
                SemanticIndex = pInfo.SemanticIndex,
                InputSlot = 0, // This does not need to be set. A shader has a single layout, 
                InstanceDataStepRate = 0, // This does not need to be set. The data is set via Context.DrawInstanced + vertex data/layout.
                AlignedByteOffset = 16 * pInfo.Register,
                InputSlotClass = InputClassification.PerVertexData,
                Format = format.ToApi()
            };
        }

        protected override void OnDispose()
        {
            // Dispose of element string pointers, since they were statically-allocated by Silk.NET
            for (uint i = 0; i < Elements.Length; i++)
                SilkMarshal.Free((nint)Elements[i].SemanticName);
        }
    }
}
