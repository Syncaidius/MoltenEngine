using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// See for info: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPipelineInputAssemblyStateCreateInfo.html
    /// </summary>
    internal unsafe class InputAssemblyStateVK : GraphicsObject<DeviceVK>, IEquatable<InputAssemblyStateVK>, IEquatable<PipelineInputAssemblyStateCreateInfo>
    {
        PipelineInputAssemblyStateCreateInfo* _desc;

        public unsafe InputAssemblyStateVK(DeviceVK device, ref ShaderPassParameters parameters) :
            base(device)
        {
            _desc = EngineUtil.Alloc<PipelineInputAssemblyStateCreateInfo>();
            _desc[0] = new PipelineInputAssemblyStateCreateInfo()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = parameters.Topology.ToApi(),
                PrimitiveRestartEnable = false,
                Flags = 0,
                PNext = null,
            };
        }

        public override bool Equals(object obj) => obj switch
        {
            InputAssemblyStateVK val => Equals(*val._desc),
            PipelineInputAssemblyStateCreateInfo val => Equals(val),
            _ => false
        };

        public bool Equals(InputAssemblyStateVK other)
        {
            return Equals(*other._desc);
        }

        public bool Equals(PipelineInputAssemblyStateCreateInfo other)
        {
            return _desc->Topology == other.Topology
                && _desc->Flags == other.Flags
                && _desc->PrimitiveRestartEnable.Value == other.PrimitiveRestartEnable.Value;
        }

        protected override void OnGraphicsRelease()
        {
            EngineUtil.Free(ref _desc);
        }

        internal PipelineInputAssemblyStateCreateInfo* Desc => _desc;
    }
}
