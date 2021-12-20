using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class GraphicsDepthStage : PipeStateStage<GraphicsDepthState, ID3D11DepthStencilState>
    {
        internal GraphicsDepthStage(PipeDX11 pipe) : base(pipe) { }

        protected override void BindState(GraphicsDepthState state)
        {
            state = state ?? Device.DepthBank.GetPreset(DepthStencilPreset.Default);
            Pipe.Context->OMSetDepthStencilState(state.NativePtr, state.StencilReference);
        }
    }
}
