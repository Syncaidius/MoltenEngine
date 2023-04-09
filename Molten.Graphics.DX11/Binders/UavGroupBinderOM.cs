using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>
    /// Binds UAVs to the output-merger (OM).
    /// </summary>
    internal unsafe class UavGroupBinderOM : GraphicsGroupBinder<GraphicsResource>
    {
        internal UavGroupBinderOM() { }

        public override void Bind(GraphicsSlotGroup<GraphicsResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = grp[sid].BoundValue != null ? (ID3D11UnorderedAccessView*)grp[sid].BoundValue.UAV : null;
                sid++;
            }

            (grp.Cmd as GraphicsQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                GraphicsQueueDX11.D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                startIndex, numChanged, pUavs);
        }

        public override void Bind(GraphicsSlot<GraphicsResource> slot, GraphicsResource value)
        {
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[1];
            pUavs[0] = slot.BoundValue != null ? (ID3D11UnorderedAccessView*)slot.BoundValue.UAV : null;

            (slot.Cmd as GraphicsQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                GraphicsQueueDX11.D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                slot.SlotIndex, 1, pUavs);
        }

        public override void Unbind(GraphicsSlotGroup<GraphicsResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            // Set unordered access resources
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[(int)numChanged];

            uint sid = startIndex;
            for (int i = 0; i < numChanged; i++)
            {
                pUavs[i] = null;
            }

            (grp.Cmd as GraphicsQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                GraphicsQueueDX11.D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                startIndex, numChanged, pUavs);
        }

        public override void Unbind(GraphicsSlot<GraphicsResource> slot, GraphicsResource value)
        {
            ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[1];
            pUavs[0] = null; 
            
            (slot.Cmd as GraphicsQueueDX11).Native->OMGetRenderTargetsAndUnorderedAccessViews(
                GraphicsQueueDX11.D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null,
                slot.SlotIndex, 1, pUavs);
        }
    }
}
