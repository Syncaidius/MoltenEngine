using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class VertexBufferGroupBinder : GraphicsGroupBinder<IGraphicsBufferSegment>
    {
        public override void Bind(GraphicsSlotGroup<IGraphicsBufferSegment> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            int iNumChanged = (int)numChanged;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[iNumChanged];
            uint* pStrides = stackalloc uint[iNumChanged];
            uint* pOffsets = stackalloc uint[iNumChanged];
            uint p = 0;
            BufferSegment seg = null;

            for (uint i = startIndex; i <= endIndex; i++)
            {
                seg = grp[i].BoundValue as BufferSegment;

                if (seg != null)
                {
                    GraphicsBuffer buffer = seg.Buffer as GraphicsBuffer;
                    if ((buffer.BufferBindFlags & BindFlag.VertexBuffer) != BindFlag.VertexBuffer)
                        throw new InvalidOperationException($"The buffer segment in vertex buffer slot {i} is not part of a vertex buffer.");

                    pBuffers[p] = buffer.ResourcePtr;
                    pStrides[p] = seg.Stride;
                    pOffsets[p] = seg.ByteOffset;
                }
                else
                {
                    pBuffers[p] = null;
                    pStrides[p] = 0;
                    pOffsets[p] = 0;
                }

                p++;
            }

            (grp.Cmd as CommandQueueDX11).Native->IASetVertexBuffers(startIndex, numChanged, pBuffers, pStrides, pOffsets);
        }

        public override void Bind(GraphicsSlot<IGraphicsBufferSegment> slot, IGraphicsBufferSegment value)
        {
            BufferSegment seg = slot.BoundValue as BufferSegment;
            GraphicsBuffer buffer = seg.Buffer as GraphicsBuffer;

            if ((buffer.BufferBindFlags & BindFlag.VertexBuffer) != BindFlag.VertexBuffer)
                throw new InvalidOperationException($"The buffer segment in vertex buffer slot {slot.SlotIndex} is not part of a vertex buffer.");

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[1];
            uint* pStrides = stackalloc uint[1];
            uint* pOffsets = stackalloc uint[1];

            if (seg != null)
            {
                pBuffers[0] = seg;
                pStrides[0] = seg.Stride;
                pOffsets[0] = seg.ByteOffset;
            }
            else
            {
                pBuffers[0] = null;
                pStrides[0] = 0;
                pOffsets[0] = 0;
            }

            (slot.Cmd as CommandQueueDX11).Native->IASetVertexBuffers(slot.SlotIndex, 0, pBuffers, pStrides, pOffsets);
        }

        public override void Unbind(GraphicsSlotGroup<IGraphicsBufferSegment> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            int iNumChanged = (int)numChanged;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[iNumChanged];
            uint* pStrides = stackalloc uint[iNumChanged];
            uint* pOffsets = stackalloc uint[iNumChanged];
            uint p = 0;

            for (uint i = startIndex; i <= endIndex; i++)
            {
                pBuffers[p] = null;
                pStrides[p] = 0;
                pOffsets[p] = 0;

                p++;
            }

            (grp.Cmd as CommandQueueDX11).Native->IASetVertexBuffers(startIndex, numChanged, pBuffers, pStrides, pOffsets);
        }

        public override void Unbind(GraphicsSlot<IGraphicsBufferSegment> slot, IGraphicsBufferSegment value)
        {
            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[1];
            uint* pStrides = stackalloc uint[1];
            uint* pOffsets = stackalloc uint[1];

            pBuffers[0] = null;
            pStrides[0] = 0;
            pOffsets[0] = 0;

            (slot.Cmd as CommandQueueDX11).Native->IASetVertexBuffers(slot.SlotIndex, 0, pBuffers, pStrides, pOffsets);
        }
    }
}
