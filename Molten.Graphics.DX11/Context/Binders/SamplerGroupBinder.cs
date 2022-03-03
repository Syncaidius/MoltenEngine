using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class SamplerGroupBinder : ContextGroupBinder<ShaderSampler>
    {
        ContextShaderStage _stage;

        internal SamplerGroupBinder(ContextShaderStage stage)
        {
            _stage = stage;
        }

        internal override void Bind(ContextSlotGroup<ShaderSampler> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[(int)numChanged];

            uint sid = startIndex;
            for (uint i = 0; i < numChanged; i++)
                samplers[i] = grp[sid++];

            _stage.SetSamplers(startIndex, numChanged, samplers);
        }

        internal override void Bind(ContextSlot<ShaderSampler> slot, ShaderSampler value)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[1];
            samplers[0] = slot.BoundValue;
            _stage.SetSamplers(slot.SlotIndex, 1, samplers);
        }

        internal override void Unbind(ContextSlotGroup<ShaderSampler> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                samplers[i] = null;

            _stage.SetSamplers(startIndex, numChanged, samplers);
        }

        internal override void Unbind(ContextSlot<ShaderSampler> slot, ShaderSampler value)
        {
            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[1];
            samplers[0] = null;
            _stage.SetSamplers(slot.SlotIndex, 1, samplers);
        }
    }
}
