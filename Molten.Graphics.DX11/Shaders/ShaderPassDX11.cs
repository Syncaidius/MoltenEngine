using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

public class ShaderPassDX11 : ShaderPass
{
    BlendStateDX11 _stateBlend;
    DepthStateDX11 _stateDepth;
    RasterizerStateDX11 _stateRasterizer;
    unsafe void* _inputByteCode;

    public ShaderPassDX11(Shader shader, string name) : base(shader, name) { }

    protected override void OnInitialize(ShaderPassParameters parameters)
    {
        // Check for unsupported features
        if (parameters.RasterizerDiscardEnabled)
            throw new NotSupportedException($"DirectX 11 mode does not support enabling of '{nameof(ShaderPassParameters.RasterizerDiscardEnabled)}'");

        DeviceDX11 device = Device as DeviceDX11;

        _stateBlend = new BlendStateDX11(device, ref parameters);
        Device.Cache.Check(ref _stateBlend);

        _stateRasterizer = new RasterizerStateDX11(device, ref parameters);
        Device.Cache.Check(ref _stateRasterizer);

        _stateDepth = new DepthStateDX11(device, ref parameters);
        Device.Cache.Check(ref _stateDepth);
    }

    internal DepthStateDX11 DepthState => _stateDepth;

    internal RasterizerStateDX11 RasterizerState => _stateRasterizer;

    internal BlendStateDX11 BlendState => _stateBlend;

    /// <summary>
    /// Gets a pointer to the bytecode that contains the input layout.
    /// </summary>
    internal unsafe ref void* InputByteCode => ref _inputByteCode;
}
