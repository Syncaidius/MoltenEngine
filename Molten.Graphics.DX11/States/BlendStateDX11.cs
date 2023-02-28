using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="CommandQueueDX11"/>.</summary>
    public unsafe class BlendStateDX11 : GraphicsObject<ID3D11BlendState1>
    {
        ID3D11BlendState1* _native;

        public BlendStateDX11(DeviceDX11 device, ref BlendDesc1 desc, Color4 blendFactor, uint blendMask) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            BlendFactor = blendFactor;
            BlendSampleMask = blendMask;
            device.Ptr->CreateBlendState1(ref desc, ref _native);
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

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

        public override unsafe ID3D11BlendState1* NativePtr => _native;

        internal Color4 BlendFactor { get; }

        internal uint BlendSampleMask { get; }
    }
}
