using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

/// <summary>Stores a blend state for use with a <see cref="GraphicsQueueDX11"/>.</summary>
public unsafe class BlendStateDX11 : GraphicsObject<DeviceDX11>, IEquatable<BlendStateDX11>, IEquatable<BlendStateDX11.CombinedDesc>
{
    public struct CombinedDesc
    {
        public BlendDesc1 Desc;

        public Color4 BlendFactor;

        public uint BlendSampleMask;
    }

    CombinedDesc _desc;
    ID3D11BlendState1* _native;

    public BlendStateDX11(DeviceDX11 device, ref ShaderPassParameters parameters) :
        base(device)
    {
        _desc = new CombinedDesc()
        { 
            BlendFactor = parameters.BlendFactor,
            BlendSampleMask = parameters.BlendSampleMask,
            Desc = new BlendDesc1()
            {
                IndependentBlendEnable = parameters.IndependentBlendEnable,
                AlphaToCoverageEnable = parameters.AlphaToCoverageEnable,
            },
        };

        for (int i = 0; i < ShaderPassParameters.MAX_SURFACES; i++)
        {
            ref RenderTargetBlendDesc1 sBlend = ref _desc.Desc.RenderTarget[i];
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

        device.Ptr->CreateBlendState1(_desc.Desc, ref _native);
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _native);
    }

    public override bool Equals(object obj) => obj switch
    {
        BlendStateDX11 state => Equals(state),
        CombinedDesc desc => Equals(desc),
        _ => false,
    };

    public bool Equals(BlendStateDX11 other)
    {
        return Equals(other._desc);
    }

    public bool Equals(CombinedDesc other)
    {
        ref BlendDesc1 a = ref _desc.Desc;
        ref BlendDesc1 b = ref other.Desc;

        if (_desc.BlendSampleMask != other.BlendSampleMask
            || _desc.BlendFactor != other.BlendFactor
            || a.AlphaToCoverageEnable.Value != b.AlphaToCoverageEnable.Value
            || a.IndependentBlendEnable.Value != b.IndependentBlendEnable.Value)
            return false;

        // Compare surface blend descriptions.
        for(int i = 0; i < ShaderPassParameters.MAX_SURFACES; i++)
        {
            ref RenderTargetBlendDesc1 blendA = ref _desc.Desc.RenderTarget[i];
            ref RenderTargetBlendDesc1 blendB = ref other.Desc.RenderTarget[i];

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

    public static implicit operator ID3D11BlendState1*(BlendStateDX11 state)
    {
        return state._native;
    }

    public static implicit operator ID3D11BlendState*(BlendStateDX11 state)
    {
        return (ID3D11BlendState*)state._native;
    }

    internal unsafe ref ID3D11BlendState1* NativePtr => ref _native;

    internal ref Color4 BlendFactor => ref _desc.BlendFactor;

    internal ref uint BlendSampleMask => ref _desc.BlendSampleMask;
}
