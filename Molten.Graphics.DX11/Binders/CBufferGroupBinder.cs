using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class CBufferGroupBinder<T> : GraphicsGroupBinder<ShaderConstantBuffer>
        where T : unmanaged
    {
        ContextShaderStage<T> _stage;

        internal CBufferGroupBinder(ContextShaderStage<T> stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlotGroup<ShaderConstantBuffer> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            int nChanged = (int)numChanged;
            ID3D11Buffer** cBuffers = stackalloc ID3D11Buffer*[nChanged];
            uint* cFirstConstants = stackalloc uint[nChanged];
            uint* cNumConstants = stackalloc uint[nChanged];

            uint sid = startIndex;
            ShaderConstantBuffer cb = null;

            for (uint i = 0; i < numChanged; i++)
            {
                cb = grp[sid++].BoundValue;
                if (cb != null)
                {
                    cBuffers[i] = cb.ResourcePtr;
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

        public override void Bind(GraphicsSlot<ShaderConstantBuffer> slot, ShaderConstantBuffer value)
        {
            ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[1];
            buffers[0] = slot.BoundValue;
            _stage.SetConstantBuffers(slot.SlotIndex, 1, buffers);
        }

        public override void Unbind(GraphicsSlotGroup<ShaderConstantBuffer> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                buffers[i] = null;

            _stage.SetConstantBuffers(startIndex, numChanged, buffers);
        }

        public override void Unbind(GraphicsSlot<ShaderConstantBuffer> slot, ShaderConstantBuffer value)
        {
            ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[1];
            buffers[0] = null;
            _stage.SetConstantBuffers(slot.SlotIndex, 1, buffers);
        }
    }
}
