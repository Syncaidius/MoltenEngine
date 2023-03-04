using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    /// <summary>
    /// See for info: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPipelineInputAssemblyStateCreateInfo.html
    /// </summary>
    internal class InputAssemblyStateVK : GraphicsObject
    {
        internal StructKey<PipelineInputAssemblyStateCreateInfo> Desc { get; }

        public InputAssemblyStateVK(GraphicsDevice device, ref GraphicsStateParameters parameters) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<PipelineInputAssemblyStateCreateInfo>();
            ref PipelineInputAssemblyStateCreateInfo desc = ref Desc.Value;
            desc.SType = StructureType.PipelineInputAssemblyStateCreateInfo;
            desc.Topology = parameters.Topology.ToApi();
            desc.PrimitiveRestartEnable = false;
        }

        public override void GraphicsRelease()
        {
            Desc.Dispose();
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
