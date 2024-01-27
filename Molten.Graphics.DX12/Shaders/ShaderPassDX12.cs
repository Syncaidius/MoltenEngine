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
        Device.Cache.Object(ref _stateRasterizer);

        _stateBlend = new BlendStateDX12(device, ref parameters);
        Device.Cache.Object(ref _stateBlend);

        _stateDepth = new DepthStateDX12(device, ref parameters);
        Device.Cache.Object(ref _stateDepth);
    }

    internal unsafe ShaderBytecode GetBytecode(ShaderType type)
    {
        ShaderComposition comp = this[type];
        if (comp == null)
            return default;

        ShaderBytecode* ptr = (ShaderBytecode*)comp.PtrShader;
        return *ptr;
    }

    internal RasterizerStateDX12 RasterizerState => _stateRasterizer;

    internal BlendStateDX12 BlendState => _stateBlend;

    internal DepthStateDX12 DepthState => _stateDepth;
}
