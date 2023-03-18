using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class ResourceGroupBinder : GraphicsGroupBinder<ResourceDX11>
    {
        ShaderStageDX11 _stage;

        internal ResourceGroupBinder(ShaderStageDX11 stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlotGroup<ResourceDX11> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[(int)numChanged];

            uint sid = startIndex;
            for (uint i = 0; i < numChanged; i++) {
                res[i] = grp[sid].BoundValue != null ? grp[sid].BoundValue.SRV.Ptr : null;
                sid++;
            }

            _stage.SetResources(startIndex, numChanged, res);
        }

        public override void Bind(GraphicsSlot<ResourceDX11> slot, ResourceDX11 value)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[1];
            res[0] = slot.BoundValue != null ? slot.BoundValue.SRV.Ptr : null;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }

        public override void Unbind(GraphicsSlotGroup<ResourceDX11> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                res[i] = null;

            _stage.SetResources(startIndex, numChanged, res);
        }

        public override void Unbind(GraphicsSlot<ResourceDX11> slot, ResourceDX11 value)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[1];
            res[0] = null;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }
    }
}
