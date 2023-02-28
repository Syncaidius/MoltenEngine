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

        BlendStateVK _blendState;
        DepthStateVK _depthState;
        RasterizerStateVK _rasterizerState;

        internal PipelineStateVK(GraphicsDevice device, ref GraphicsStateParameters parameters) : 
            base(device)
        {
            _info = new StructKey<GraphicsPipelineCreateInfo>();
            _info.Value.SType = StructureType.GraphicsPipelineCreateInfo;

            StructKey<PipelineDepthStencilStateCreateInfo> descDepth = new StructKey<PipelineDepthStencilStateCreateInfo>();
            StructKey<PipelineRasterizationStateCreateInfo> descRaster = new StructKey<PipelineRasterizationStateCreateInfo>();
            StructKey<PipelineColorBlendStateCreateInfo> descBlend = new StructKey<PipelineColorBlendStateCreateInfo>();

            // TODO populate blend description
            ref PipelineColorBlendStateCreateInfo bDesc = ref descBlend.Value;
            bDesc.Flags = PipelineColorBlendStateCreateFlags.None; // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPipelineColorBlendStateCreateFlagBits.html

            Color4 blendConsts = parameters.BlendFactor;
            bDesc.SType = StructureType.PipelineColorBlendStateCreateInfo;
            bDesc.BlendConstants[0] = blendConsts.R;
            bDesc.BlendConstants[1] = blendConsts.G;
            bDesc.BlendConstants[2] = blendConsts.B;
            bDesc.BlendConstants[3] = blendConsts.A;
            bDesc.LogicOp = parameters.Surface0.LogicOp.ToApi();
            bDesc.LogicOpEnable = parameters.Surface0.LogicOpEnable;
            bDesc.AttachmentCount = GraphicsStateParameters.MAX_SURFACES;
            bDesc.PAttachments = EngineUtil.AllocArray<PipelineColorBlendAttachmentState>(bDesc.AttachmentCount);
            for(uint i = 0; i < bDesc.AttachmentCount; i++)
            {
                ref PipelineColorBlendAttachmentState at = ref bDesc.PAttachments[i];
                // TODO set per-surface blend state
            }


            // TODO populate rasterizer description
            ref PipelineRasterizationStateCreateInfo raDesc = ref descRaster.Value;
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
            ref PipelineDepthStencilStateCreateInfo dDesc = ref descDepth.Value;
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

            _blendState = new BlendStateVK(device, descBlend);
            _blendState = device.CacheObject(descBlend, _blendState);

            _depthState = new DepthStateVK(device, descDepth);
            _depthState = device.CacheObject(descDepth, _depthState);

            _rasterizerState = new RasterizerStateVK(device, descRaster);
            _rasterizerState = device.CacheObject(descRaster, _rasterizerState);
        }

        public override void GraphicsRelease()
        {

        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
