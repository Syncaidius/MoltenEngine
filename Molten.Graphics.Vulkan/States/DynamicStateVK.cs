using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class DynamicStateVK : GraphicsObject, IEquatable<DynamicStateVK>, IEquatable<PipelineDynamicStateCreateInfo>
    {
        PipelineDynamicStateCreateInfo* _desc;

        /// <summary>
        ///
        /// </summary>
        /// <param name="device"></param>
        /// <param name="parameters"></param>
        /// <param name="states">A list of state values to say which parts of a piepline state will be dynamic.</param>
        public DynamicStateVK(GraphicsDevice device, ref ShaderPassParameters parameters, DynamicState[] states) :
            base(device)
        {
            _desc = EngineUtil.Alloc<PipelineDynamicStateCreateInfo>();
            _desc[0] = new PipelineDynamicStateCreateInfo()
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = (uint)states.Length,
                PDynamicStates = EngineUtil.AllocArray<DynamicState>((uint)states.Length),
                Flags = 0,
                PNext = null,
            };

            uint numBytes = sizeof(DynamicState) * _desc->DynamicStateCount;
            fixed (DynamicState* ptr = &states[0])
                System.Buffer.MemoryCopy(&ptr, _desc->PDynamicStates, numBytes, numBytes);
        }

        public override bool Equals(object obj) => obj switch
        {
            DynamicStateVK other => Equals(*other._desc),
            PipelineDynamicStateCreateInfo other => Equals(other),
            _ => base.Equals(obj),
        };

        public bool Equals(DynamicStateVK other)
        {
            return Equals(*other._desc);
        }

        public bool Equals(PipelineDynamicStateCreateInfo other)
        {
            return _desc->DynamicStateCount == other.DynamicStateCount
                && _desc->Flags == other.Flags
                && _desc->PDynamicStates == other.PDynamicStates;
        }

        protected override void OnGraphicsRelease()
        {
            EngineUtil.Free(ref _desc->PDynamicStates);
            EngineUtil.Free(ref _desc);
        }

        internal PipelineDynamicStateCreateInfo* Desc => _desc;
    }
}
