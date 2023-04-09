using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="GraphicsQueueDX11"/>.</summary>
    public unsafe class BlendStateDX11 : GraphicsObject
    {
        internal StructKey<BlendDesc1> Desc { get; }

        ID3D11BlendState1* _native;

        public BlendStateDX11(DeviceDX11 device, ref ShaderPassParameters parameters) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<BlendDesc1>();
            ref BlendDesc1 bDesc = ref Desc.Value;
            bDesc.IndependentBlendEnable = parameters.IndependentBlendEnable;
            bDesc.AlphaToCoverageEnable = parameters.AlphaToCoverageEnable;

            for (int i = 0; i < ShaderPassParameters.MAX_SURFACES; i++)
            {
                ref RenderTargetBlendDesc1 sBlend = ref bDesc.RenderTarget[i];
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

            BlendFactor = parameters.BlendFactor;
            BlendSampleMask = parameters.BlendSampleMask;
            device.Ptr->CreateBlendState1(Desc, ref _native);
        }

        protected override void OnApply(GraphicsQueue cmd) { }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
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

        internal Color4 BlendFactor { get; }

        internal uint BlendSampleMask { get; }
    }
}
