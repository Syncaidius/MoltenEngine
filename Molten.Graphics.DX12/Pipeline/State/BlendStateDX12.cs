using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

/// <summary>Stores a blend state description.</summary>
public unsafe class BlendStateDX12 : GpuObject<DeviceDX12>, IEquatable<BlendStateDX12>, IEquatable<BlendStateDX12.CombinedDesc>
{
    public struct CombinedDesc
    {
        public BlendDesc Desc;

        public Color4 BlendFactor;

        public uint BlendSampleMask;
    }

    CombinedDesc _desc;

    public BlendStateDX12(DeviceDX12 device, ref ShaderPassParameters parameters) :
        base(device)
    {
        _desc = new CombinedDesc()
        { 
            BlendFactor = parameters.BlendFactor,
            BlendSampleMask = parameters.BlendSampleMask,
            Desc = new BlendDesc()
            {
                IndependentBlendEnable = parameters.IndependentBlendEnable,
                AlphaToCoverageEnable = parameters.AlphaToCoverageEnable,
            },
        };

        for (int i = 0; i < ShaderPassParameters.MAX_SURFACES; i++)
        {
            ref RenderTargetBlendDesc sBlend = ref _desc.Desc.RenderTarget[i];
            ShaderPassParameters.SurfaceBlend pBlend = parameters[i];

            sBlend.BlendEnable = pBlend.BlendEnable;
            sBlend.SrcBlend = pBlend.SrcBlend.ToApi();
            sBlend.SrcBlendAlpha = pBlend.SrcBlendAlpha.ToApi();
            sBlend.DestBlend = pBlend.DestBlend.ToApi();
            sBlend.DestBlendAlpha = pBlend.DestBlendAlpha.ToApi();
            sBlend.RenderTargetWriteMask = (byte)pBlend.RenderTargetWriteMask;
            sBlend.LogicOp = (LogicOp)pBlend.LogicOp;
            sBlend.LogicOpEnable = pBlend.LogicOpEnable;
            sBlend.BlendOp = (BlendOp)pBlend.BlendOp;
            sBlend.BlendOpAlpha = (BlendOp)pBlend.BlendOpAlpha;

            parameters[i] = pBlend;
        }
    }

    protected override void OnGraphicsRelease() { }

    public override bool Equals(object obj) => obj switch
    {
        BlendStateDX12 state => Equals(state),
        CombinedDesc desc => Equals(desc),
        _ => false,
    };

    public bool Equals(BlendStateDX12 other)
    {
        return Equals(other._desc);
    }

    public bool Equals(CombinedDesc other)
    {
        ref BlendDesc a = ref _desc.Desc;
        ref BlendDesc b = ref other.Desc;

        if (_desc.BlendSampleMask != other.BlendSampleMask
            || _desc.BlendFactor != other.BlendFactor
            || a.AlphaToCoverageEnable.Value != b.AlphaToCoverageEnable.Value
            || a.IndependentBlendEnable.Value != b.IndependentBlendEnable.Value)
            return false;

        // Compare surface blend descriptions.
        for(int i = 0; i < ShaderPassParameters.MAX_SURFACES; i++)
        {
            ref RenderTargetBlendDesc blendA = ref _desc.Desc.RenderTarget[i];
            ref RenderTargetBlendDesc blendB = ref other.Desc.RenderTarget[i];

            if(blendA.DestBlend != blendB.DestBlend
                || blendA.DestBlendAlpha != blendB.DestBlendAlpha
                || blendA.SrcBlend != blendB.SrcBlend
                || blendA.SrcBlendAlpha != blendB.SrcBlendAlpha
                || blendA.BlendEnable.Value != blendB.BlendEnable.Value
                || blendA.BlendOp != blendB.BlendOp
                || blendA.BlendOpAlpha != blendB.BlendOpAlpha
                || blendA.LogicOp != blendB.LogicOp
                || blendA.LogicOpEnable.Value != blendB.LogicOpEnable.Value
                || blendA.RenderTargetWriteMask != blendB.RenderTargetWriteMask)
                return false;
        }

        return true;
    }

    internal ref readonly CombinedDesc Description => ref _desc;

    internal ref readonly Color4 BlendFactor => ref _desc.BlendFactor;

    internal ref readonly uint BlendSampleMask => ref _desc.BlendSampleMask;
}
