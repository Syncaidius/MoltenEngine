using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="GraphicsQueueDX11"/>.</summary>
    internal unsafe class DepthStateDX11 : GraphicsObject
    {
        internal StructKey<DepthStencilDesc> Desc { get; }

        ID3D11DepthStencilState* _native;
        uint _stencilReference;

        internal DepthStateDX11(DeviceDX11 device, ref ShaderPassParameters parameters) :
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<DepthStencilDesc>();

            ref DepthStencilDesc dDesc = ref Desc.Value;
            dDesc.DepthEnable = parameters.IsDepthEnabled;
            dDesc.DepthFunc = (ComparisonFunc)parameters.DepthComparison;
            dDesc.DepthWriteMask = parameters.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;
            dDesc.StencilWriteMask = parameters.StencilWriteMask;
            dDesc.StencilReadMask = parameters.StencilReadMask;

            _stencilReference = parameters.DepthFrontFace.StencilReference > 0 ? parameters.DepthFrontFace.StencilReference : parameters.DepthBackFace.StencilReference;
            dDesc.FrontFace = new DepthStencilopDesc()
            {
                StencilDepthFailOp = (StencilOp)parameters.DepthFrontFace.DepthFail,
                StencilFailOp = (StencilOp)parameters.DepthFrontFace.StencilFail,
                StencilFunc = (ComparisonFunc)parameters.DepthFrontFace.Comparison,
                StencilPassOp = (StencilOp)parameters.DepthFrontFace.StencilPass,
            };
            dDesc.BackFace = new DepthStencilopDesc()
            {
                StencilDepthFailOp = (StencilOp)parameters.DepthBackFace.DepthFail,
                StencilFailOp = (StencilOp)parameters.DepthBackFace.StencilFail,
                StencilFunc = (ComparisonFunc)parameters.DepthBackFace.Comparison,
                StencilPassOp = (StencilOp)parameters.DepthBackFace.StencilPass,
            };

            device.Ptr->CreateDepthStencilState(Desc, ref _native);
        }

        protected override void OnApply(GraphicsQueue cmd) { }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
            Desc.Dispose();
        }

        internal unsafe ref ID3D11DepthStencilState* NativePtr => ref _native;

        public uint StencilReference { get; set; }
    }
}
