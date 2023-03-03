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
        DynamicStateVK _dynamicState;

        internal PipelineStateVK(DeviceVK device, ref GraphicsStateParameters parameters) : 
            base(device)
        {
            // Populate dynamic state

            _blendState = new BlendStateVK(device, ref parameters);
            _blendState = device.CacheObject(_blendState.Desc, _blendState);

            _depthState = new DepthStateVK(device, ref parameters);
            _depthState = device.CacheObject(_depthState.Desc, _depthState);

            _rasterizerState = new RasterizerStateVK(device, ref parameters);
            _rasterizerState = device.CacheObject(_rasterizerState.Desc, _rasterizerState);

            DynamicState[] dynamics = new DynamicState[]
            {
                DynamicState.ViewportWithCount,
                DynamicState.ScissorWithCount,
            };

            _dynamicState = new DynamicStateVK(device, ref parameters, dynamics);
            _dynamicState = device.CacheObject(_dynamicState.Desc, _dynamicState);

            _info = new StructKey<GraphicsPipelineCreateInfo>();
            ref GraphicsPipelineCreateInfo pInfo = ref _info.Value;
            pInfo.SType = StructureType.GraphicsPipelineCreateInfo;
            pInfo.Flags = PipelineCreateFlags.None;

            pInfo.PMultisampleState = null;                         // TODO initialize
            pInfo.PInputAssemblyState = null;                       // TODO initialize
            pInfo.Layout = new PipelineLayout();                    // TODO initialize
            pInfo.BasePipelineIndex = 0;                            // TODO initialize
            pInfo.BasePipelineHandle = new Pipeline();              // TODO initialize
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
            pInfo.PDynamicState = _dynamicState.Desc;
        }

        public override void GraphicsRelease()
        {

        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
