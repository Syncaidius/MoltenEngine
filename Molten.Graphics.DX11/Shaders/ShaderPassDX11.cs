using Silk.NET.Core.Native;

namespace Molten.Graphics.DX11
{
    public class ShaderPassDX11 : HlslPass
    {
        public ShaderPassDX11(HlslShader shader, string name) : base(shader, name) { }

        protected override void OnInitialize(ref ShaderPassParameters parameters)
        {
            // Check for unsupported features
            if (parameters.RasterizerDiscardEnabled)
                throw new NotSupportedException($"DirectX 11 mode does not support enabling of '{nameof(ShaderPassParameters.RasterizerDiscardEnabled)}'");

            DeviceDX11 device = Device as DeviceDX11;

            BlendState = new BlendStateDX11(device, ref parameters);
            BlendState = Device.CacheObject(BlendState.Desc, BlendState);

            RasterizerState = new RasterizerStateDX11(device, ref parameters);
            RasterizerState = Device.CacheObject(RasterizerState.Desc, RasterizerState);

            DepthState = new DepthStateDX11(device, ref parameters);
            DepthState = Device.CacheObject(DepthState.Desc, DepthState);

            Topology = parameters.Topology.ToApi();
        }

        internal DepthStateDX11 DepthState { get; private set; }

        internal RasterizerStateDX11 RasterizerState { get; private set; }

        internal BlendStateDX11 BlendState { get; private set; }

        internal D3DPrimitiveTopology Topology { get; private set; }
    }
}
