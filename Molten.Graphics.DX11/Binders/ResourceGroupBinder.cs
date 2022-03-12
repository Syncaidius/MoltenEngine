using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ResourceGroupBinder<T> : ContextGroupBinder<ContextBindableResource>
        where T : unmanaged
    {
        ContextShaderStage<T> _stage;

        internal ResourceGroupBinder(ContextShaderStage<T> stage)
        {
            _stage = stage;
        }

        internal override void Bind(ContextSlotGroup<ContextBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[(int)numChanged];

            uint sid = startIndex;
            for (uint i = 0; i < numChanged; i++) {
                res[i] = grp[sid].BoundValue != null ? grp[sid].BoundValue.SRV.NativePtr : null;
                sid++;
            }

            _stage.SetResources(startIndex, numChanged, res);
        }

        internal override void Bind(ContextSlot<ContextBindableResource> slot, ContextBindableResource value)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[1];
            res[0] = slot.BoundValue != null ? slot.BoundValue.SRV.NativePtr : null;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }

        internal override void Unbind(ContextSlotGroup<ContextBindableResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                res[i] = null;

            _stage.SetResources(startIndex, numChanged, res);
        }

        internal override void Unbind(ContextSlot<ContextBindableResource> slot, ContextBindableResource value)
        {
            ID3D11ShaderResourceView** res = stackalloc ID3D11ShaderResourceView*[1];
            res[0] = null;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }
    }
}
