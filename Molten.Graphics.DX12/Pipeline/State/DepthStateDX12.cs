using Silk.NET.Direct3D12;
using System.Runtime.InteropServices;

namespace Molten.Graphics.DX12;

/// <summary>Stores a depth-stencil state description.</summary>
internal unsafe class DepthStateDX12 : GpuObject<DeviceDX12>, IEquatable<DepthStateDX12>, IEquatable<DepthStateDX12.CombinedDesc>
{
    [StructLayout(LayoutKind.Explicit)]
    public struct CombinedDesc
    {
        const int DESC1_SIZE = 56;

        [FieldOffset(0)]
        public DepthStencilDesc1 Desc1;

        [FieldOffset(0)]
        public DepthStencilDesc Desc;

        [FieldOffset(DESC1_SIZE)]
        public uint StencilReference;
    }

    CombinedDesc _desc;

    internal DepthStateDX12(DeviceDX12 device, ref ShaderPassParameters parameters) :
        base(device)
    {
        _desc = new CombinedDesc();
        ref DepthStencilDesc1 dDesc = ref _desc.Desc1;
        dDesc.DepthEnable = parameters.IsDepthEnabled;
        dDesc.DepthFunc = (ComparisonFunc)parameters.DepthComparison;
        dDesc.DepthWriteMask = parameters.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;
        dDesc.StencilWriteMask = parameters.StencilWriteMask;
        dDesc.StencilReadMask = parameters.StencilReadMask;
        dDesc.StencilEnable = parameters.IsStencilEnabled;
        dDesc.DepthBoundsTestEnable = parameters.DepthBoundsTestEnabled;
        _desc.StencilReference = parameters.DepthFrontFace.StencilReference > 0 ? 
            parameters.DepthFrontFace.StencilReference : 
            parameters.DepthBackFace.StencilReference;

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
        if(obj is DepthStateDX12 other)
            return Equals(other._desc);

        return false;
    }

    public bool Equals(CombinedDesc other)
    {
        if (_desc.StencilReference != other.StencilReference)
            return false;

        ref DepthStencilDesc1 a = ref _desc.Desc1;
        ref DepthStencilDesc1 b = ref other.Desc1;
        if (a.StencilEnable.Value != b.StencilEnable.Value
            || a.DepthEnable.Value != b.DepthEnable.Value
            || a.DepthFunc != b.DepthFunc
            || a.DepthWriteMask != b.DepthWriteMask
            || a.StencilReadMask != b.StencilReadMask
            || a.StencilWriteMask != b.StencilWriteMask
            || a.StencilEnable != b.StencilEnable
            || !StencilOpEqual(ref a.FrontFace, ref b.FrontFace)
            || !StencilOpEqual(ref a.BackFace, ref b.BackFace))
            return false;

        return true;
    }

    public bool Equals(DepthStateDX12 other)
    {
        return Equals(other._desc);
    }

    protected override void OnGraphicsRelease() { }

    internal ref readonly CombinedDesc Description => ref _desc;

    public uint StencilReference { get; set; }
}
