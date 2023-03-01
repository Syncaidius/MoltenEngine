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

        internal PipelineStateVK(DeviceVK device, ref GraphicsStateParameters parameters) : 
            base(device)
        {
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
                GraphicsStateParameters.SurfaceBlend sBlend = parameters[i];

                at.BlendEnable = sBlend.BlendEnable;
                at.SrcColorBlendFactor = sBlend.SrcBlend.ToApi();
                at.DstColorBlendFactor = sBlend.DestBlend.ToApi();
                at.ColorBlendOp = sBlend.BlendOp.ToApi();
                at.SrcAlphaBlendFactor = sBlend.SrcBlendAlpha.ToApi();
                at.DstAlphaBlendFactor = sBlend.DestBlendAlpha.ToApi();
                at.AlphaBlendOp = sBlend.BlendOpAlpha.ToApi();
                at.ColorWriteMask = sBlend.RenderTargetWriteMask.ToApi();
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

            _info = new StructKey<GraphicsPipelineCreateInfo>();
            ref GraphicsPipelineCreateInfo pInfo = ref _info.Value;
            pInfo.SType = StructureType.GraphicsPipelineCreateInfo;
            pInfo.Flags = PipelineCreateFlags.None;

            pInfo.PMultisampleState = null;                         // TODO initialize
            pInfo.PInputAssemblyState = null;                       // TODO initialize
            pInfo.Layout = new PipelineLayout();                    // TODO initialize
            pInfo.BasePipelineIndex = 0;                            // TODO initialize
            pInfo.BasePipelineHandle = new Pipeline();              // TODO initialize
            pInfo.PDynamicState = null;                             // TODO initialize
            pInfo.PTessellationState = null;                        // TODO initialize
            pInfo.PVertexInputState = null;                         // TODO initialize
            pInfo.PViewportState = null;                            // Ignored since need to be able to change the viewport
            pInfo.RenderPass = new RenderPass();                    // TODO initialize
            pInfo.PStages = null;                                   // TODO initialize
            pInfo.StageCount = 0;                                   // TODO initialize
            pInfo.Subpass = 0;                                      // TODO initialize

            pInfo.PColorBlendState = _blendState.Desc;
            pInfo.PRasterizationState = _rasterizerState.Desc;
            pInfo.PDepthStencilState = _depthState.Desc;

            // TODO populate the other parts of a vulkan pipeline state

        }

        public override void GraphicsRelease()
        {

        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
