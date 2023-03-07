using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>
    /// Binds UAVs to the output-merger (OM).
    /// </summary>
    internal unsafe class UavGroupBinderOM : GraphicsGroupBinder<GraphicsResourceDX11>
    {
        /// <summary>
        ///  If you set NumRTVs to D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL (0xffffffff), 
        ///  this method does not modify the currently bound render-target views (RTVs) and also does not modify depth-stencil view (DSV).
        /// </summary>
        const uint D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL = 0xffffffff;

        internal UavGroupBinderOM()
        {
            
        }

        public override void Bind(GraphicsSlotGroup<GraphicsResourceDX11> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = grp[sid].BoundValue != null ? (ID3D11UnorderedAccessView*)grp[sid].BoundValue.UAV.Ptr : null;
                sid++;
            }

            (grp.Cmd as CommandQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                startIndex, numChanged, pUavs);
        }

        public override void Bind(GraphicsSlot<GraphicsResourceDX11> slot, GraphicsResourceDX11 value)
        {
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[1];
            pUavs[0] = slot.BoundValue != null ? (ID3D11UnorderedAccessView*)slot.BoundValue.UAV.Ptr : null;

            (slot.Cmd as CommandQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                slot.SlotIndex, 1, pUavs);
        }

        public override void Unbind(GraphicsSlotGroup<GraphicsResourceDX11> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = null;
            }

            (grp.Cmd as CommandQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                startIndex, numChanged, pUavs);
        }

        public override void Unbind(GraphicsSlot<GraphicsResourceDX11> slot, GraphicsResourceDX11 value)
        {
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[1];
            pUavs[0] = null; 
            
            (slot.Cmd as CommandQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                slot.SlotIndex, 1, pUavs);
        }
    }
}
