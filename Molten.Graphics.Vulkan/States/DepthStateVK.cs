using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class DepthStateVK : GraphicsObject
    {
        internal StructKey<PipelineDepthStencilStateCreateInfo> Desc { get; }

        public DepthStateVK(GraphicsDevice device, StructKey<PipelineDepthStencilStateCreateInfo> desc) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = desc;
        }

        public override void GraphicsRelease()
        {
            Desc.Dispose();
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
