using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

/// <summary>Stores a rasterizer state description.</summary>
internal unsafe class RasterizerStateDX12 : GraphicsObject<DeviceDX12>, IEquatable<RasterizerStateDX12>, IEquatable<RasterizerDesc>
{
    RasterizerDesc _desc;

    /// <summary>
    /// Creates a new instance of <see cref="RasterizerStateDX11"/>.
    /// </summary>
    /// <param name="device">The <see cref="DeviceDX11"/> to use when creating the underlying rasterizer state object.</param>
    /// <param name="desc"></param>
    internal RasterizerStateDX12(DeviceDX12 device, ref ShaderPassParameters parameters) : 
        base(device)
    {
        _desc = new RasterizerDesc();
        _desc.MultisampleEnable = parameters.IsMultisampleEnabled;
        _desc.DepthClipEnable = parameters.IsDepthClipEnabled;
        _desc.AntialiasedLineEnable = parameters.IsAALineEnabled;
        _desc.FillMode = parameters.FillMode.ToApi();
        _desc.CullMode = parameters.CullMode.ToApi();
        _desc.DepthBias = parameters.DepthBiasEnabled ? parameters.DepthBias : 0;
        _desc.DepthBiasClamp = parameters.DepthBiasEnabled ? parameters.DepthBiasClamp : 0;
        _desc.SlopeScaledDepthBias = parameters.SlopeScaledDepthBias;
        _desc.ConservativeRaster = (ConservativeRasterizationMode)parameters.ConservativeRaster;
        _desc.ForcedSampleCount = parameters.ForcedSampleCount;
        _desc.FrontCounterClockwise = parameters.IsFrontCounterClockwise;
    }

    public override bool Equals(object obj) => obj switch
    {
        RasterizerStateDX12 state => Equals(state._desc),
        RasterizerDesc desc => Equals(desc),
        _ => false
    };

    public bool Equals(RasterizerStateDX12 other)
    {
        return Equals(other._desc);
    }

    public bool Equals(RasterizerDesc other)
    {
        return _desc.MultisampleEnable.Value == other.MultisampleEnable &&
            _desc.DepthClipEnable.Value == other.DepthClipEnable &&
            _desc.AntialiasedLineEnable.Value == other.AntialiasedLineEnable &&
            _desc.FillMode == other.FillMode &&
            _desc.CullMode == other.CullMode &&
            _desc.DepthBias == other.DepthBias &&
            _desc.DepthBiasClamp == other.DepthBiasClamp &&
            _desc.SlopeScaledDepthBias == other.SlopeScaledDepthBias &&
            _desc.ConservativeRaster == other.ConservativeRaster &&
            _desc.ForcedSampleCount == other.ForcedSampleCount &&
            _desc.FrontCounterClockwise.Value == other.FrontCounterClockwise;
    }

    protected override void OnGraphicsRelease() { }

    internal ref readonly RasterizerDesc Desc => ref _desc;
}
