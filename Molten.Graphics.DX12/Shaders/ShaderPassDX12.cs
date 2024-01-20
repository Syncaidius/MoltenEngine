using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public class ShaderPassDX12 : HlslPass
{
    public ShaderPassDX12(HlslShader parent, string name) : 
        base(parent, name) { }

    RasterizerStateDX12 _stateRasterizer;
    BlendStateDX12 _stateBlend;
    DepthStateDX12 _stateDepth;

    protected override void OnInitialize(ref ShaderPassParameters parameters)
    {
        DeviceDX12 device = Device as DeviceDX12;

        _stateRasterizer = new RasterizerStateDX12(device, ref parameters);
        Device.Cache.Object<RasterizerStateDX12, RasterizerDesc>(ref _stateRasterizer);

        _stateBlend = new BlendStateDX12(device, ref parameters);
        Device.Cache.Object<BlendStateDX12, BlendStateDX12.CombinedDesc>(ref _stateBlend);

        _stateDepth = new DepthStateDX12(device, ref parameters);
        Device.Cache.Object<DepthStateDX12, DepthStateDX12.CombinedDesc>(ref _stateDepth);
    }

    internal RasterizerStateDX12 RasterizerState => _stateRasterizer;

    internal BlendStateDX12 BlendState => _stateBlend;

    internal DepthStateDX12 DepthState => _stateDepth;
}
