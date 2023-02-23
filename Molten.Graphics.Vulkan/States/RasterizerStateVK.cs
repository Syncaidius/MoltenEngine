using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class RasterizerStateVK : GraphicsObject
    {
        internal StructKey<PipelineRasterizationStateCreateInfo> Desc { get; }

        public RasterizerStateVK(GraphicsDevice device, StructKey<PipelineRasterizationStateCreateInfo> desc) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<PipelineRasterizationStateCreateInfo>(desc);
            Desc.Value.SType = StructureType.PipelineRasterizationStateCreateInfo;
        }

        public override void GraphicsRelease()
        {
            Desc.Dispose();
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
