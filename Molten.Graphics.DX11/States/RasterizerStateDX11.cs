using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="CommandQueueDX11"/>.</summary>
    internal unsafe class RasterizerStateDX11 : GraphicsObject
    {
        public unsafe ID3D11RasterizerState2* NativePtr => _native;

        internal StructKey<RasterizerDesc2> Desc { get; }

        ID3D11RasterizerState2* _native;

        /// <summary>
        /// Creates a new instance of <see cref="RasterizerStateDX11"/>.
        /// </summary>
        /// <param name="source">An existing <see cref="RasterizerStateDX11"/> instance from which to copy settings."/></param>
        internal RasterizerStateDX11(GraphicsDevice device, StructKey<RasterizerDesc2> desc) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<RasterizerDesc2>(desc);
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_native == null)
            {
                (cmd as CommandQueueDX11).DXDevice.Ptr->CreateRasterizerState2(Desc, ref _native);
                Version++;
            }
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public static implicit operator ID3D11RasterizerState2* (RasterizerStateDX11 state)
        {
            return state._native;
        }

        public static implicit operator ID3D11RasterizerState*(RasterizerStateDX11 state)
        {
            return (ID3D11RasterizerState*)state._native;
        }
    }
}
