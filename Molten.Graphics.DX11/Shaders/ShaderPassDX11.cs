using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    public class ShaderPassDX11 : HlslPass
    {
        BlendStateDX11 _stateBlend;
        DepthStateDX11 _stateDepth;
        RasterizerStateDX11 _stateRasterizer;

        public ShaderPassDX11(HlslShader shader, string name) : base(shader, name) { }

        protected override void OnInitialize(ref ShaderPassParameters parameters)
        {
            // Check for unsupported features
            if (parameters.RasterizerDiscardEnabled)
                throw new NotSupportedException($"DirectX 11 mode does not support enabling of '{nameof(ShaderPassParameters.RasterizerDiscardEnabled)}'");

            DeviceDX11 device = Device as DeviceDX11;

            _stateBlend = new BlendStateDX11(device, ref parameters);
            Device.Cache.Object<BlendStateDX11, BlendStateDX11.CombinedDesc>(ref _stateBlend);

            _stateRasterizer = new RasterizerStateDX11(device, ref parameters);
            Device.Cache.Object<RasterizerStateDX11, RasterizerDesc2>(ref _stateRasterizer);

            _stateDepth = new DepthStateDX11(device, ref parameters);
            Device.Cache.Object<DepthStateDX11, DepthStateDX11.CombinedDesc>(ref _stateDepth);
        }

        internal DepthStateDX11 DepthState => _stateDepth;

        internal RasterizerStateDX11 RasterizerState => _stateRasterizer;

        internal BlendStateDX11 BlendState => _stateBlend;
    }
}
