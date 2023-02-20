using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="CommandQueueDX11"/>.</summary>
    internal unsafe class DepthStateDX11 : GraphicsObject<ID3D11DepthStencilState>
    {        
        internal StructKey<DepthStencilDesc> Desc { get; }

        ID3D11DepthStencilState* _native;

        internal DepthStateDX11(DeviceDX11 device, StructKey<DepthStencilDesc> desc) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<DepthStencilDesc>(desc);
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_native == null)
            {
                (cmd as CommandQueueDX11).DXDevice.Ptr->CreateDepthStencilState(Desc, ref _native);
                Version++;
            }
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
            Desc.Dispose();
        }

        public override unsafe ID3D11DepthStencilState* NativePtr => _native;
    }
}
