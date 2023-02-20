using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="CommandQueueDX11"/>.</summary>
    public unsafe class BlendStateDX11 : GraphicsObject<ID3D11BlendState1>
    {
        internal StructKey<BlendDesc1> Key { get; }

        ID3D11BlendState1* _native;

        public BlendStateDX11(DeviceDX11 device, StructKey<BlendDesc1> key) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Key = new StructKey<BlendDesc1>(key);
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_native == null)
            {
                (cmd as CommandQueueDX11).DXDevice.Ptr->CreateBlendState1(Key, ref _native);
                Version++;
            }
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
            Key.Dispose();
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
    }
}
