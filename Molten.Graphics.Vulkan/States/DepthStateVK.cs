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

        public DepthStateVK(GraphicsDevice device, ref GraphicsStateParameters parameters) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<PipelineDepthStencilStateCreateInfo>();

            ref PipelineDepthStencilStateCreateInfo dDesc = ref Desc.Value;
            dDesc.SType = StructureType.PipelineDepthStencilStateCreateInfo;
            dDesc.DepthTestEnable = parameters.IsDepthEnabled;
            dDesc.StencilTestEnable = parameters.IsStencilEnabled;
            dDesc.DepthWriteEnable = parameters.DepthWriteEnabled;
            dDesc.DepthBoundsTestEnable = parameters.DepthBoundsTestEnabled;
            dDesc.MaxDepthBounds = parameters.MaxDepthBounds;
            dDesc.MinDepthBounds = parameters.MinDepthBounds;
            dDesc.DepthCompareOp = parameters.DepthComparison.ToApi();
            dDesc.Front = new StencilOpState()
            {
                CompareMask = parameters.StencilReadMask,
                WriteMask = parameters.StencilWriteMask,
                CompareOp = parameters.DepthFrontFace.Comparison.ToApi(),
                DepthFailOp = parameters.DepthFrontFace.DepthFail.ToApi(),
                FailOp = parameters.DepthFrontFace.StencilFail.ToApi(),
                PassOp = parameters.DepthFrontFace.StencilPass.ToApi(),
                Reference = parameters.DepthFrontFace.StencilReference
            };
            dDesc.Back = new StencilOpState()
            {
                CompareMask = parameters.StencilWriteMask,
                WriteMask = parameters.StencilWriteMask,
                CompareOp = parameters.DepthBackFace.Comparison.ToApi(),
                DepthFailOp = parameters.DepthBackFace.DepthFail.ToApi(),
                FailOp = parameters.DepthBackFace.StencilFail.ToApi(),
                PassOp = parameters.DepthBackFace.StencilPass.ToApi(),
                Reference = parameters.DepthFrontFace.StencilReference
            };
        }

        public override void GraphicsRelease()
        {
            Desc.Dispose();
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
