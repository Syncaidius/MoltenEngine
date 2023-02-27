using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="CommandQueueDX11"/>.</summary>
    internal unsafe class RasterizerStateDX11 : GraphicsObject<ID3D11RasterizerState2>
    {
        public override unsafe ID3D11RasterizerState2* NativePtr => _native;

        ID3D11RasterizerState2* _native;

        /// <summary>
        /// Creates a new instance of <see cref="RasterizerStateDX11"/>.
        /// </summary>
        /// <param name="device">The <see cref="DeviceDX11"/> to use when creating the underlying rasterizer state object.</param>
        /// <param name="desc"></param>
        internal RasterizerStateDX11(DeviceDX11 device, ref RasterizerDesc2 desc) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            device.Ptr->CreateRasterizerState2(ref desc, ref _native);
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public static implicit operator ID3D11RasterizerState*(RasterizerStateDX11 state)
        {
            return (ID3D11RasterizerState*)state._native;
        }
    }
}
