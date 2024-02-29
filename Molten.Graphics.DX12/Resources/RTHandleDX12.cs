using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class RTHandleDX12 : ResourceHandleDX12
{
    internal unsafe RTHandleDX12(TextureDX12 texture, params ID3D12Resource1*[] resources) : 
        base(texture, resources)
    {
        RTV = new RTViewDX12(this);
    }

    internal unsafe RTHandleDX12(TextureDX12 texture, ID3D12Resource1** resources, uint numResources) : 
        base(texture, resources, numResources)
    {
        RTV = new RTViewDX12(this);
    }

    public override void Dispose()
    {
        RTV.Dispose();
        base.Dispose();
    }

    internal RTViewDX12 RTV { get; }
}
