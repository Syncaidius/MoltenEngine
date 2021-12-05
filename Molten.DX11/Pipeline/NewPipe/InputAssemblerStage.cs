using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class InputAssemblerStage : PipeStage
    {
        VertexTopology _boundTopology;

        /* TODO:
         *  - Add MaterialBinder which replicates the MaterialInputStage functionality of old pipeline
         *  - Add VertexInputLayout functionality
         *  - 
         * 
         * 
         */
        public InputAssemblerStage(PipeDX11 pipe, PipeStageType stageType) : 
            base(pipe, stageType)
        {
            uint maxVBuffers = pipe.Device.Features.MaxVertexBufferSlots;
            VertexBuffers = DefineSlotGroup<PipeBufferSegment>(maxVBuffers, PipeBindTypeFlags.Input, "V-Buffer");
            IndexBuffer = DefineSlot<PipeBufferSegment>(1, PipeBindTypeFlags.Input, "I-Buffer");
        }

        internal override void Bind()
        {
            // Check topology
            if (_boundTopology != Topology)
            {
                _boundTopology = Topology;
                Pipe.Context->IASetPrimitiveTopology(_boundTopology.ToApi());
            }

            if (VertexBuffers.BindAll(BindVertexBuffers))
            {

            }

            // Check index buffer
            if (IndexBuffer.Bind())
            {
                PipeBufferSegment ib = IndexBuffer.BoundValue;
                Pipe.Context->IASetIndexBuffer(ib.Buffer.Native, ib.DataFormat, ib.ByteOffset);
            }
        }

        private void BindVertexBuffers(PipeBindSlot<PipeBufferSegment>[] slots, uint startSlot, uint endSlot, uint numChanged)
        {
            int iNumChanged = (int)numChanged;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[iNumChanged];
            uint* pStrides = stackalloc uint[iNumChanged];
            uint* pOffsets = stackalloc uint[iNumChanged];
            uint p = 0;
            PipeBufferSegment seg = null;
            
            for (uint i = startSlot; i <= endSlot; i++)
            {
                seg = slots[i].BoundValue;

                if (seg != null)
                {
                    pBuffers[p] = seg.Buffer.Native;
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

            Pipe.Context->IASetVertexBuffers(startSlot, numChanged, pBuffers, pStrides, pOffsets);
        }

        public PipeBindSlotGroup<PipeBufferSegment> VertexBuffers { get; }

        public PipeBindSlot<PipeBufferSegment> IndexBuffer { get;}

        public VertexTopology Topology { get; set; }
    }
}
