using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="CommandQueueDX11"/>.</summary>
    internal unsafe class DepthStateDX11 : GraphicsObject<ID3D11DepthStencilState>
    {        
        ID3D11DepthStencilState* _native;
        uint _stencilReference;

        internal DepthStateDX11(DeviceDX11 device, ref DepthStencilDesc desc, uint stencilRef) :
            base(device, GraphicsBindTypeFlags.Input)
        {
            _stencilReference = stencilRef;
            device.Ptr->CreateDepthStencilState(ref desc, ref _native);
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public override unsafe ID3D11DepthStencilState* NativePtr => _native;

        public uint StencilReference { get; set; }
    }
}
