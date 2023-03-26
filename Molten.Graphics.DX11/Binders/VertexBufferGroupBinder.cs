using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class VertexBufferGroupBinder : GraphicsGroupBinder<IVertexBuffer>
    {
        public override void Bind(GraphicsSlotGroup<IVertexBuffer> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            int iNumChanged = (int)numChanged;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[iNumChanged];
            uint* pStrides = stackalloc uint[iNumChanged];
            uint* pOffsets = stackalloc uint[iNumChanged];
            uint p = 0;
            VertexBufferDX11 buffer = null;

            for (uint i = startIndex; i <= endIndex; i++)
            {
                buffer = grp[i].BoundValue as VertexBufferDX11;

                if (buffer != null)
                {
                    pBuffers[p] = (ID3D11Buffer*)buffer.Handle;
                    pStrides[p] = buffer.Stride;
                    pOffsets[p] = 0; // buffer.ByteOffset; - May need again for multi-part meshes with sub-meshes within the same buffer.
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

        public override void Bind(GraphicsSlot<IVertexBuffer> slot, IVertexBuffer value)
        {
            VertexBufferDX11 buffer = slot.BoundValue as VertexBufferDX11;

            if (!(buffer is IVertexBuffer))
                throw new InvalidOperationException($"The buffer in vertex buffer slot {slot.SlotIndex} is not a vertex buffer.");

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[1];
            uint* pStrides = stackalloc uint[1];
            uint* pOffsets = stackalloc uint[1];

            if (buffer != null)
            {
                pBuffers[0] = (ID3D11Buffer*)buffer.Handle;
                pStrides[0] = buffer.Stride;
                pOffsets[0] = 0; // buffer.ByteOffset; - May need again for multi-part meshes with sub-meshes within the same buffer.
            }
            else
            {
                pBuffers[0] = null;
                pStrides[0] = 0;
                pOffsets[0] = 0;
            }

            (slot.Cmd as CommandQueueDX11).Native->IASetVertexBuffers(slot.SlotIndex, 1, pBuffers, pStrides, pOffsets);
        }

        public override void Unbind(GraphicsSlotGroup<IVertexBuffer> grp, uint startIndex, uint endIndex, uint numChanged)
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

        public override void Unbind(GraphicsSlot<IVertexBuffer> slot, IVertexBuffer value)
        {
            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[1];
            uint* pStrides = stackalloc uint[1];
            uint* pOffsets = stackalloc uint[1];

            pBuffers[0] = null;
            pStrides[0] = 0;
            pOffsets[0] = 0;

            (slot.Cmd as CommandQueueDX11).Native->IASetVertexBuffers(slot.SlotIndex, 1, pBuffers, pStrides, pOffsets);
        }
    }
}
