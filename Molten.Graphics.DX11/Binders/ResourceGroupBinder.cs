using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal unsafe class ResourceGroupBinder : GraphicsGroupBinder<GraphicsResource>
    {
        ShaderStageDX11 _stage;

        internal ResourceGroupBinder(ShaderStageDX11 stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlotGroup<GraphicsResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[(int)numChanged];

            uint sid = startIndex;
            for (uint i = 0; i < numChanged; i++) {
                res[i] = grp[sid].BoundValue != null ? (ID3D11ShaderResourceView1*)grp[sid].BoundValue.SRV : null;
                sid++;
            }

            _stage.SetResources(startIndex, numChanged, res);
        }

        public override void Bind(GraphicsSlot<GraphicsResource> slot, GraphicsResource value)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[1];
            res[0] = slot.BoundValue != null ? (ID3D11ShaderResourceView1*)slot.BoundValue.SRV : null;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }

        public override void Unbind(GraphicsSlotGroup<GraphicsResource> grp, uint startIndex, uint endIndex, uint numChanged)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[(int)numChanged];

            for (uint i = 0; i < numChanged; i++)
                res[i] = null;

            _stage.SetResources(startIndex, numChanged, res);
        }

        public override void Unbind(GraphicsSlot<GraphicsResource> slot, GraphicsResource value)
        {
            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[1];
            res[0] = null;
            _stage.SetResources(slot.SlotIndex, 1, res);
        }
    }
}
