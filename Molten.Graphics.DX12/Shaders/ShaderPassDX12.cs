using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public class ShaderPassDX12 : ShaderPass, IEquatable<ShaderPassDX12>
{
    public ShaderPassDX12(Shader parent, string name) : 
        base(parent, name) { }

    RasterizerStateDX12 _stateRasterizer;
    BlendStateDX12 _stateBlend;
    DepthStateDX12 _stateDepth;

    protected override void OnInitialize(ref ShaderPassParameters parameters)
    {
        DeviceDX12 device = Device as DeviceDX12;

        _stateRasterizer = new RasterizerStateDX12(device, ref parameters);
        Device.Cache.Check(ref _stateRasterizer);

        _stateBlend = new BlendStateDX12(device, ref parameters);
        Device.Cache.Check(ref _stateBlend);

        _stateDepth = new DepthStateDX12(device, ref parameters);
        Device.Cache.Check(ref _stateDepth);
    }

    internal unsafe ShaderBytecode GetBytecode(ShaderType type)
    {
        ShaderComposition comp = this[type];
        if (comp == null)
            return default;

        ShaderBytecode* ptr = (ShaderBytecode*)comp.PtrShader;
        return *ptr;
    }

    public bool Equals(ShaderPassDX12 other)
    {
        return this == other 
            || (_stateRasterizer == other._stateRasterizer
            && _stateBlend == other._stateBlend
            && _stateDepth == other._stateDepth);
    }

    public override bool Equals(object obj)
    {
        if(obj is ShaderPassDX12 pass)
            return Equals(pass);
        else
            return false;
    }

    internal RasterizerStateDX12 RasterizerState => _stateRasterizer;

    internal BlendStateDX12 BlendState => _stateBlend;

    internal DepthStateDX12 DepthState => _stateDepth;
}
