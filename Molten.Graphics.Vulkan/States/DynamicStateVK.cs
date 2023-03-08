using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
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
            desc.DynamicStateCount = (uint)states.Length;
            desc.PDynamicStates = EngineUtil.AllocArray<DynamicState>(desc.DynamicStateCount);

            uint numBytes = sizeof(DynamicState) * desc.DynamicStateCount;
            fixed(DynamicState* ptr = &states[0])
                System.Buffer.MemoryCopy(&ptr, desc.PDynamicStates, numBytes, numBytes);
        }

        public override void GraphicsRelease()
        {
            DynamicState* dPtr = Desc.Value.PDynamicStates;
            Desc.Dispose();
            EngineUtil.Free(ref dPtr);
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
