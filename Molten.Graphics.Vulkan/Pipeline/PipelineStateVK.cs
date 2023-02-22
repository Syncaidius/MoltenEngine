using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class PipelineStateVK : GraphicsObject
    {
        GraphicsPipelineCreateInfo* _info;
        Pipeline _pipeline;
        GraphicsDepthState _state;

        internal PipelineStateVK(GraphicsDevice device) : base(device, GraphicsBindTypeFlags.Input)
        {
            _info = EngineUtil.Alloc<GraphicsPipelineCreateInfo>();
        }

        public override void GraphicsRelease()
        {
            throw new NotImplementedException();
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            // TODO if dirty, recreate _pipeline.

            /* TODO refactor GraphicsCommandQueue to only have a single State slot.
             *  - GraphicsCommandQueue.ApplyState() should become abstract
             *  - Each API should apply the state object in it's own way
             *  - GraphicsPipelineState will be created to contain
             *      - Vertex input layout
             *      - Depth, Rasterizer and blend states
             *      - Viewport state/info
             *      - API-specific state info
             *      - Most features listed here: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkGraphicsPipelineCreateInfo.html
             *      - In DX11 this could all be stored in a Deferred Context
             *  - Note that some parts of a pipeline state will need updating dynamically per frame or during resizes:
             *  - See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDynamicState.html
             *      - Viewports
             *      - Scissor rects
             *      - Stencil reference value
             *  - Each Renderable will store a GraphicsPipelineState on it
             *      - Dynamic parts will update each time the Renderable is rendered, as above
             *  - If a state is modified (e.g. a depth state was reassigned) then the state will be recreated prior to next usage.
             *  
             *  NOTE: This may 
            */
        }
    }
}
