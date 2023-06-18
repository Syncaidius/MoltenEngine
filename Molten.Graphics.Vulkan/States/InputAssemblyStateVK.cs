using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// See for info: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPipelineInputAssemblyStateCreateInfo.html
    /// </summary>
    internal class InputAssemblyStateVK : GraphicsObject
    {
        internal StructKey<PipelineInputAssemblyStateCreateInfo> Desc { get; }

        public unsafe InputAssemblyStateVK(GraphicsDevice device, ref ShaderPassParameters parameters) : 
            base(device)
        {
            Desc = new StructKey<PipelineInputAssemblyStateCreateInfo>();
            ref PipelineInputAssemblyStateCreateInfo desc = ref Desc.Value;
            desc.SType = StructureType.PipelineInputAssemblyStateCreateInfo;
            desc.PNext = null;
            desc.Topology = parameters.Topology.ToApi();
            desc.PrimitiveRestartEnable = false;
        }

        protected override void OnGraphicsRelease()
        {
            Desc.Dispose();
        }
    }
}
