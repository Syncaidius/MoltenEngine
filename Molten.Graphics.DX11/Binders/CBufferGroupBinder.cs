using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal unsafe class CBufferGroupBinder : GraphicsGroupBinder<ConstantBufferDX11>
    {
        ShaderStageDX11 _stage;

        internal CBufferGroupBinder(ShaderStageDX11 stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlotGroup<ConstantBufferDX11> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            int nChanged = (int)numChanged;
            ID3D11Buffer** cBuffers = stackalloc ID3D11Buffer*[nChanged];
            uint* cFirstConstants = stackalloc uint[nChanged];
            uint* cNumConstants = stackalloc uint[nChanged];

            uint sid = startIndex;
            ConstantBufferDX11 cb = null;

            for (uint i = 0; i < numChanged; i++)
            {
                cb = grp[sid++].BoundValue;
                if (cb != null)
                {
                    cBuffers[i] = (ID3D11Buffer*)cb.Handle;
                    cFirstConstants[i] = 0; // TODO implement this using BufferSegment
                    cNumConstants[i] = (uint)cb.Variables.Length;
                }
                else
                {
                    cBuffers[i] = null;
                    cFirstConstants[i] = 0;
                    cNumConstants[i] = 0;
                }
            }

            _stage.SetConstantBuffers(startIndex, numChanged, cBuffers);
        }

        public override void Bind(GraphicsSlot<ConstantBufferDX11> slot, ConstantBufferDX11 value)
        {
            ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[1];
            buffers[0] = (ID3D11Buffer*)slot.BoundValue.Handle;
            _stage.SetConstantBuffers(slot.SlotIndex, 1, buffers);
        }

        public override void Unbind(GraphicsSlotGroup<ConstantBufferDX11> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                buffers[i] = null;

            _stage.SetConstantBuffers(startIndex, numChanged, buffers);
        }

        public override void Unbind(GraphicsSlot<ConstantBufferDX11> slot, ConstantBufferDX11 value)
        {
            ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[1];
            buffers[0] = null;
            _stage.SetConstantBuffers(slot.SlotIndex, 1, buffers);
        }
    }
}
