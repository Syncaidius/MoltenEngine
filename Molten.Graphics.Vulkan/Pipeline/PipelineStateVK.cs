using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class PipelineStateVK : GraphicsState
    {
        StructKey<GraphicsPipelineCreateInfo> _info;
        Pipeline _pipeline;

        StructKey<PipelineDepthStencilStateCreateInfo> _descDepth;
        DepthStateVK _depthState;

        StructKey<PipelineRasterizationStateCreateInfo> _descRasterizer;
        RasterizerStateVK _rasterizerState;

        internal PipelineStateVK(GraphicsDevice device, ref GraphicsStateParameters parameters) : 
            base(device)
        {
            _info = new StructKey<GraphicsPipelineCreateInfo>();
            _info.Value.SType = StructureType.GraphicsPipelineCreateInfo;

            _descDepth = new StructKey<PipelineDepthStencilStateCreateInfo>();
            _descRasterizer = new StructKey<PipelineRasterizationStateCreateInfo>();

            // TODO populate blend description

            // TODO populate rasterizer description
            ref PipelineRasterizationStateCreateInfo raDesc = ref _descRasterizer.Value;
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

            // Populate depth-stencil description
            ref PipelineDepthStencilStateCreateInfo dDesc = ref _descDepth.Value;
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
            _descDepth.Dispose();
            _descRasterizer.Dispose();
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
