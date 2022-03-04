using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class VertexBufferGroupBinder : ContextGroupBinder<BufferSegment>
    {
        internal override void Bind(ContextSlotGroup<BufferSegment> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            int iNumChanged = (int)numChanged;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[iNumChanged];
            uint* pStrides = stackalloc uint[iNumChanged];
            uint* pOffsets = stackalloc uint[iNumChanged];
            uint p = 0;
            BufferSegment seg = null;

            for (uint i = startIndex; i <= endIndex; i++)
            {
                seg = grp[i].BoundValue;

                if (((BindFlag)seg.Buffer.Description.BindFlags & BindFlag.BindVertexBuffer) != BindFlag.BindVertexBuffer)
                    throw new InvalidOperationException($"The buffer segment in vertex buffer slot {i} is not part of a vertex buffer.");

                if (seg != null)
                {
                    pBuffers[p] = seg.Buffer.ResourcePtr;
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

            grp.Context.Native->IASetVertexBuffers(startIndex, numChanged, pBuffers, pStrides, pOffsets);
        }

        internal override void Bind(ContextSlot<BufferSegment> slot, BufferSegment value)
        {
            BufferSegment seg = slot.BoundValue;

            if (((BindFlag)seg.Buffer.Description.BindFlags & BindFlag.BindVertexBuffer) != BindFlag.BindVertexBuffer)
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

            slot.Context.Native->IASetVertexBuffers(slot.SlotIndex, 0, pBuffers, pStrides, pOffsets);
        }

        internal override void Unbind(ContextSlotGroup<BufferSegment> grp, uint startIndex, uint endIndex, uint numChanged)
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

            grp.Context.Native->IASetVertexBuffers(startIndex, numChanged, pBuffers, pStrides, pOffsets);
        }

        internal override void Unbind(ContextSlot<BufferSegment> slot, BufferSegment value)
        {
            BufferSegment seg = slot.BoundValue;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[1];
            uint* pStrides = stackalloc uint[1];
            uint* pOffsets = stackalloc uint[1];

            pBuffers[0] = null;
            pStrides[0] = 0;
            pOffsets[0] = 0;

            slot.Context.Native->IASetVertexBuffers(slot.SlotIndex, 0, pBuffers, pStrides, pOffsets);
        }
    }
}
