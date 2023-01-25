using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class SamplerGroupBinder<T> : GraphicsGroupBinder<ShaderSampler>
        where T: unmanaged
    {
        ContextShaderStage<T> _stage;

        internal SamplerGroupBinder(ContextShaderStage<T> stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlotGroup<ShaderSampler> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[(int)numChanged];

            uint sid = startIndex;
            for (uint i = 0; i < numChanged; i++)
                samplers[i] = grp[sid++].BoundValue;

            _stage.SetSamplers(startIndex, numChanged, samplers);
        }

        public override void Bind(GraphicsSlot<ShaderSampler> slot, ShaderSampler value)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[1];
            samplers[0] = slot.BoundValue;
            _stage.SetSamplers(slot.SlotIndex, 1, samplers);
        }

        public override void Unbind(GraphicsSlotGroup<ShaderSampler> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                samplers[i] = null;

            _stage.SetSamplers(startIndex, numChanged, samplers);
        }

        public override void Unbind(GraphicsSlot<ShaderSampler> slot, ShaderSampler value)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[1];
            samplers[0] = null;
            _stage.SetSamplers(slot.SlotIndex, 1, samplers);
        }
    }
}
