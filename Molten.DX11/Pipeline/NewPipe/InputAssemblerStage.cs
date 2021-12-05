using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class InputAssemblerStage : PipeStage
    {
        VertexBufferBinder _binder;

        VertexTopology _boundTopology;

        public InputAssemblerStage(PipeDX11 pipe, PipeStageType stageType) : 
            base(pipe, stageType)
        {
            _binder = new VertexBufferBinder(pipe);
            uint maxVBuffers = pipe.Device.Features.MaxVertexBufferSlots;
            VertexBuffers = DefineSlotGroup<BufferSegment>(maxVBuffers, PipeBindTypeFlags.Input, "V-Buffer");
            IndexBuffer = DefineSlotGroup<BufferSegment>(1, PipeBindTypeFlags.Input, "I-Buffer");
        }

        protected override void OnDispose()
        {
            _binder.Dispose();
            base.OnDispose();
        }

        internal override void Bind()
        {
            // TODO move functionality of VertexBufferBinder to here.

            // Check topology
            if (_boundTopology != Topology)
            {
                _boundTopology = Topology;
                Pipe.Context->IASetPrimitiveTopology(_boundTopology.ToApi());
            }

            // Check index buffer
            if (IndexBuffer.Bind())
            {
                BufferSegment ib = IndexBuffer.BoundValue;
                Pipe.Context->IASetIndexBuffer(ib.Buffer.Native, ib.DataFormat, ib.ByteOffset);
            }
        }

        public PipeBindSlotGroup<BufferSegment> VertexBuffers { get; }

        public PipeBindSlot<BufferSegment> IndexBuffer { get;}

        public VertexTopology Topology { get; set; }
    }
}
