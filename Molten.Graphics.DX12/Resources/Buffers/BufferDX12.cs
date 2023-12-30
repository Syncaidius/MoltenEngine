namespace Molten.Graphics.DX12.Resources.Buffers
{
    internal class BufferDX12 : GraphicsBuffer
    {
        ResourceHandleDX12<ID3D12Buffer>[] _handles;
        ResourceHandleDX12<ID3D12Buffer> _curHandle;
        protected BufferDesc Desc;

        public override GraphicsResourceHandle Handle => throw new NotImplementedException();

        public override unsafe void* SRV => throw new NotImplementedException();

        public override unsafe void* UAV => throw new NotImplementedException();

        public override GraphicsFormat ResourceFormat { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        protected override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            throw new NotImplementedException();
        }

        protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            throw new NotImplementedException();
        }

        protected override void OnGraphicsRelease()
        {
            throw new NotImplementedException();
        }

        protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
        {
            throw new NotImplementedException();
        }
    }
}
