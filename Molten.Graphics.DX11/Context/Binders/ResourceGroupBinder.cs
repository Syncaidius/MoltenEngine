using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ResourceGroupBinder : ContextGroupBinder<PipeBindableResource>
    {
        ContextShaderStage _stage;

        internal ResourceGroupBinder(ContextShaderStage stage)
        {
            _stage = stage;
        }

        internal override void Bind(ContextSlotGroup<PipeBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[(int)numChanged];

            uint sid = startIndex;
            for (uint i = 0; i < numChanged; i++)
                res[i] = grp.GetBoundValue(sid++);

            _stage.SetResources(startIndex, numChanged, res);
        }

        internal override void Bind(ContextSlot<PipeBindableResource> slot, PipeBindableResource value)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[1];
            res[0] = slot.BoundValue;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }

        internal override void Unbind(ContextSlotGroup<PipeBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                res[i] = null;

            _stage.SetResources(startIndex, numChanged, res);
        }

        internal override void Unbind(ContextSlot<PipeBindableResource> slot, PipeBindableResource value)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[1];
            res[0] = null;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }
    }
}
