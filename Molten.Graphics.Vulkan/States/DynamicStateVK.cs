using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class DynamicStateVK : GraphicsObject
    {
        internal StructKey<PipelineDynamicStateCreateInfo> Desc { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="parameters"></param>
        /// <param name="states">A list of state values to say which parts of a piepline state will be dynamic.</param>
        public DynamicStateVK(GraphicsDevice device, ref ShaderPassParameters parameters, DynamicState[] states) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<PipelineDynamicStateCreateInfo>();
            ref PipelineDynamicStateCreateInfo desc = ref Desc.Value;
            desc.SType = StructureType.PipelineDynamicStateCreateInfo;
            desc.DynamicStateCount = (uint)states.Length;
            desc.PDynamicStates = EngineUtil.AllocArray<DynamicState>(desc.DynamicStateCount);
            desc.PNext = null;

            uint numBytes = sizeof(DynamicState) * desc.DynamicStateCount;
            fixed(DynamicState* ptr = &states[0])
                System.Buffer.MemoryCopy(&ptr, desc.PDynamicStates, numBytes, numBytes);
        }

        protected override void OnGraphicsRelease()
        {
            DynamicState* dPtr = Desc.Value.PDynamicStates;
            Desc.Dispose();
            EngineUtil.Free(ref dPtr);
        }

        protected override void OnApply(GraphicsQueue queue) { }
    }
}
