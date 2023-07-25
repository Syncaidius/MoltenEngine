using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    /// <summary>Stores a rasterizer state for use with a <see cref="GraphicsQueueDX11"/>.</summary>
    internal unsafe class RasterizerStateDX11 : GraphicsObject, IEquatable<RasterizerStateDX11>, IEquatable<RasterizerDesc2>
    {
        RasterizerDesc2 _desc;
        ID3D11RasterizerState2* _native;

        /// <summary>
        /// Creates a new instance of <see cref="RasterizerStateDX11"/>.
        /// </summary>
        /// <param name="device">The <see cref="DeviceDX11"/> to use when creating the underlying rasterizer state object.</param>
        /// <param name="desc"></param>
        internal RasterizerStateDX11(DeviceDX11 device, ref ShaderPassParameters parameters) : 
            base(device)
        {
            _desc = new RasterizerDesc2();
            _desc.MultisampleEnable = parameters.IsMultisampleEnabled;
            _desc.DepthClipEnable = parameters.IsDepthClipEnabled;
            _desc.AntialiasedLineEnable = parameters.IsAALineEnabled;
            _desc.ScissorEnable = parameters.IsScissorEnabled;
            _desc.FillMode = parameters.Fill.ToApi();
            _desc.CullMode = parameters.Cull.ToApi();
            _desc.DepthBias = parameters.DepthBiasEnabled ? parameters.DepthBias : 0;
            _desc.DepthBiasClamp = parameters.DepthBiasEnabled ? parameters.DepthBiasClamp : 0;
            _desc.SlopeScaledDepthBias = parameters.SlopeScaledDepthBias;
            _desc.ConservativeRaster = (ConservativeRasterizationMode)parameters.ConservativeRaster;
            _desc.ForcedSampleCount = parameters.ForcedSampleCount;
            _desc.FrontCounterClockwise = parameters.IsFrontCounterClockwise;

            device.Ptr->CreateRasterizerState2(_desc, ref _native);
        }

        public override bool Equals(object obj) => obj switch
        {
            RasterizerStateDX11 state => Equals(state._desc),
            RasterizerDesc2 desc => Equals(desc),
            _ => false
        };

        public bool Equals(RasterizerStateDX11 other)
        {
            return Equals(other._desc);
        }

        public bool Equals(RasterizerDesc2 other)
        {
            return _desc.MultisampleEnable.Value == other.MultisampleEnable &&
                _desc.DepthClipEnable.Value == other.DepthClipEnable &&
                _desc.AntialiasedLineEnable.Value == other.AntialiasedLineEnable &&
                _desc.ScissorEnable.Value == other.ScissorEnable &&
                _desc.FillMode == other.FillMode &&
                _desc.CullMode == other.CullMode &&
                _desc.DepthBias == other.DepthBias &&
                _desc.DepthBiasClamp == other.DepthBiasClamp &&
                _desc.SlopeScaledDepthBias == other.SlopeScaledDepthBias &&
                _desc.ConservativeRaster == other.ConservativeRaster &&
                _desc.ForcedSampleCount == other.ForcedSampleCount &&
                _desc.FrontCounterClockwise.Value == other.FrontCounterClockwise;
        }

        protected override void OnGraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public static implicit operator ID3D11RasterizerState*(RasterizerStateDX11 state)
        {
            return (ID3D11RasterizerState*)state._native;
        }
    }
}
