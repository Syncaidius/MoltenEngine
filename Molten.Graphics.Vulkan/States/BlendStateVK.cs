using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class BlendStateVK : GraphicsObject
    {
        internal StructKey<PipelineColorBlendStateCreateInfo> Desc { get; }

        public BlendStateVK(GraphicsDevice device, StructKey<PipelineColorBlendStateCreateInfo> desc) : 
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
