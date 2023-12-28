using Silk.NET.Direct3D11;
using static Molten.Graphics.DX11.DepthStateDX11;

namespace Molten.Graphics.DX11
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="GraphicsQueueDX11"/>.</summary>
    internal unsafe class DepthStateDX11 : GraphicsObject<DeviceDX11>, IEquatable<DepthStateDX11>, IEquatable<CombinedDesc>
    {
        public struct CombinedDesc
        {
            public DepthStencilDesc Desc;
            public uint StencilReference;
        }

        ID3D11DepthStencilState* _native;
        CombinedDesc _desc;

        internal DepthStateDX11(DeviceDX11 device, ref ShaderPassParameters parameters) :
            base(device)
        {
            _desc = new CombinedDesc();
            ref DepthStencilDesc dDesc = ref _desc.Desc;
            dDesc.DepthEnable = parameters.IsDepthEnabled;
            dDesc.DepthFunc = (ComparisonFunc)parameters.DepthComparison;
            dDesc.DepthWriteMask = parameters.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;
            dDesc.StencilWriteMask = parameters.StencilWriteMask;
            dDesc.StencilReadMask = parameters.StencilReadMask;

            _desc.StencilReference = parameters.DepthFrontFace.StencilReference > 0 ? parameters.DepthFrontFace.StencilReference : parameters.DepthBackFace.StencilReference;
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

            device.Ptr->CreateDepthStencilState(dDesc, ref _native);
        }

        private bool StencilOpEqual(ref DepthStencilopDesc a, ref DepthStencilopDesc b)
        {
            return a.StencilDepthFailOp == b.StencilDepthFailOp
                && a.StencilFailOp == b.StencilFailOp
                && a.StencilFunc == b.StencilFunc
                && a.StencilPassOp == b.StencilPassOp;
        }

        public override bool Equals(object obj)
        {
            if(obj is DepthStateDX11 other)
                return Equals(other._desc);

            return false;
        }

        public bool Equals(CombinedDesc other)
        {
            if (_desc.StencilReference != other.StencilReference)
                return false;

            ref DepthStencilDesc a = ref _desc.Desc;
            ref DepthStencilDesc b = ref other.Desc;
            if (a.StencilEnable.Value != b.StencilEnable.Value
                || a.DepthEnable.Value != b.DepthEnable.Value
                || a.DepthFunc != b.DepthFunc
                || a.DepthWriteMask != b.DepthWriteMask
                || a.StencilReadMask != b.StencilReadMask
                || a.StencilWriteMask != b.StencilWriteMask
                || !StencilOpEqual(ref a.FrontFace, ref b.FrontFace)
                || !StencilOpEqual(ref a.BackFace, ref b.BackFace))
                return false;

            return true;
        }

        public bool Equals(DepthStateDX11 other)
        {
            return Equals(other._desc);
        }

        protected override void OnGraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        internal unsafe ref ID3D11DepthStencilState* NativePtr => ref _native;

        public uint StencilReference { get; set; }
    }
}
