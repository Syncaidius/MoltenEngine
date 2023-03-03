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

        public RasterizerStateVK(GraphicsDevice device, ref GraphicsStateParameters parameters) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<PipelineRasterizationStateCreateInfo>();

            ref PipelineRasterizationStateCreateInfo raDesc = ref Desc.Value;
            raDesc.SType = StructureType.PipelineRasterizationStateCreateInfo;
            raDesc.PolygonMode = parameters.Fill.ToApi();
            raDesc.CullMode = parameters.Cull.ToApi();
            raDesc.DepthBiasClamp = parameters.DepthBiasClamp;
            raDesc.DepthBiasSlopeFactor = parameters.SlopeScaledDepthBias;
            raDesc.DepthClampEnable = parameters.DepthBiasEnabled;
            raDesc.DepthBiasConstantFactor = parameters.DepthBias;
            raDesc.FrontFace = parameters.IsFrontCounterClockwise ? FrontFace.CounterClockwise : FrontFace.Clockwise;
            raDesc.RasterizerDiscardEnable = parameters.RasterizerDiscardEnabled;
            raDesc.LineWidth = parameters.LineWidth;
            raDesc.Flags = 0; // Reserved for use in future Vulkan versions.  
        }

        public override void GraphicsRelease()
        {
            Desc.Dispose();
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
