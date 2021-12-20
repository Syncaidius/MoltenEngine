using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class GraphicsBlendStage : PipeStateStage<GraphicsBlendState, ID3D11BlendState1>
    {
        internal GraphicsBlendStage(PipeDX11 pipe) : base(pipe) { }

        protected override void BindState(GraphicsBlendState state)
        {
            state = state ?? Device.BlendBank.GetPreset(BlendPreset.Default);
            float* pFactor = Color4.ToFloatPtr(state.BlendFactor);
            Pipe.Context->OMSetBlendState(state, pFactor, state.BlendSampleMask);
        }
    }
}
