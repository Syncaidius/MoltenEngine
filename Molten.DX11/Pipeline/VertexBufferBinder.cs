using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class VertexBufferBinder : PipelineComponent<DeviceDX11, PipeDX11>
    {
        uint _maxSlots;
        BufferSegment[] _segments;
        PipelineBindSlot<BufferSegment, DeviceDX11, PipeDX11>[] _slotVertexBuffers;
        uint _minChanged;
        uint _maxChanged;
        PipeDX11 _pipe;

        public VertexBufferBinder(PipeDX11 pipe) : base(pipe)
        {
            _pipe = pipe;
            _maxSlots = Device.Features.MaxVertexBufferSlots;

            _segments = new BufferSegment[_maxSlots];
            _slotVertexBuffers = new PipelineBindSlot<BufferSegment, DeviceDX11, PipeDX11>[_maxSlots];

            for (int i = 0; i < _maxSlots; i++)
            {
                _slotVertexBuffers[i] = AddSlot<BufferSegment>(i);
                _slotVertexBuffers[i].OnObjectForcedUnbind += PipelineInput_OnBoundObjectDisposed;
            }
        }

        private void PipelineInput_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, 
            PipelineDisposableObject obj)
        {
            Bind(slot.SlotID, null);
        }

        internal void BindToContext()
        {
            // No changes to make if _maxChanged was not set higher than _minChanged.
            if (_maxChanged < _minChanged)
                return;

            BufferSegment seg = null;
            uint bCount = _maxChanged - _minChanged;
            int ibCount = (int)bCount;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[ibCount];
            uint* pStrides = stackalloc uint[ibCount];
            uint* pOffsets = stackalloc uint[ibCount];

            uint p = 0;
            for (uint i = _minChanged; i <= _maxChanged; i++)
            {
                seg = _segments[i];

                // TODO rework binding slot system now we're not relying on SharpDX.
                bool vbChanged = _slotVertexBuffers[i].Bind(Pipe, seg, PipelineBindType.Input);

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
                bCount++;
            }

            _pipe.Context->IASetVertexBuffers(_minChanged, bCount, pBuffers, pStrides, pOffsets);

            // Reset min/max
            _minChanged = uint.MaxValue;
            _maxChanged = uint.MinValue;
        }

        public void Bind(uint slot, BufferSegment segment)
        {
            _segments[slot] = segment;

            if (segment != null)
            {
                if (segment.Buffer.Native != Buffers[slot] ||
                    segment.Stride != Strides[slot] ||
                    segment.ByteOffset != Offsets[slot])
                {
                    if (slot <= _minChanged)
                        _minChanged = slot;

                    if (slot >= _maxChanged)
                        _maxChanged = slot;
                }
            }
        }
    }
}
