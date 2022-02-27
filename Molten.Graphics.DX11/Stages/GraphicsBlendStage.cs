using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class GraphicsBlendStage : PipeStateStage<GraphicsBlendState, ID3D11BlendState>
    {
        internal GraphicsBlendStage(DeviceContext pipe) : base(pipe) { }

        protected override void BindState(GraphicsBlendState state)
        {
            state = state ?? Device.BlendBank.GetPreset(BlendPreset.Default);
            Color4 tmp = state.BlendFactor;
            Pipe.NativeContext->OMSetBlendState(state, (float*)&tmp, state.BlendSampleMask);
        }

        protected override void UnbindState(PipeSlot<GraphicsBlendState> slot)
        {
            Pipe.NativeContext->OMSetBlendState(null, null, 0);
        }
    }
}
