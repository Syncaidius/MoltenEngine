using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    internal unsafe class GraphicsQueueDX12 : GraphicsQueue
    {
        CommandQueueDesc _desc;
        ID3D12CommandQueue* _ptr;

        internal GraphicsQueueDX12(Logger log, DeviceDX12 device, DeviceBuilderDX12 builder, ref CommandQueueDesc desc) : 
            base(device)
        {
            _desc = desc;
            Device = device;
            Log = log;

            Initialize(builder);
        }

        private void Initialize(DeviceBuilderDX12 builder)
        {
            Guid cmdGuid = ID3D12CommandQueue.Guid;
            void* cmdQueue = null;
            HResult r = Device.Ptr->CreateCommandQueue(_desc, &cmdGuid, &cmdQueue);
            if (!builder.CheckResult(r))
            {
                Log.Error($"Failed to initialize '{_desc.Type}' command queue");
                return;
            }
            else
            {
                Log.WriteLine($"Initialized '{_desc.Type}' command queue");
            }

            _ptr = (ID3D12CommandQueue*)cmdQueue;
        }

        public override void Execute(GraphicsCommandList list)
        {
            throw new NotImplementedException();
        }

        public override void Submit(GraphicsCommandListFlags flags)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult Draw(HlslShader shader, uint vertexCount, uint vertexStartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult DrawInstanced(HlslShader shader, uint vertexCountPerInstance, uint instanceCount, uint vertexStartIndex = 0, uint instanceStartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult DrawIndexed(HlslShader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult DrawIndexedInstanced(HlslShader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult Dispatch(HlslShader shader, Vector3UI groups)
        {
            throw new NotImplementedException();
        }

        public override void SetRenderSurfaces(IRenderSurface2D[] surfaces, uint count)
        {
            throw new NotImplementedException();
        }

        public override void SetRenderSurface(IRenderSurface2D surface, uint slot)
        {
            throw new NotImplementedException();
        }

        public override void GetRenderSurfaces(IRenderSurface2D[] destinationArray)
        {
            throw new NotImplementedException();
        }

        public override IRenderSurface2D GetRenderSurface(uint slot)
        {
            throw new NotImplementedException();
        }

        public override void ResetRenderSurfaces()
        {
            throw new NotImplementedException();
        }

        public override void SetScissorRectangle(Rectangle rect, int slot = 0)
        {
            throw new NotImplementedException();
        }

        public override void SetScissorRectangles(params Rectangle[] rects)
        {
            throw new NotImplementedException();
        }

        public override void SetViewport(ViewportF vp, int slot)
        {
            throw new NotImplementedException();
        }

        public override void SetViewports(ViewportF vp)
        {
            throw new NotImplementedException();
        }

        public override void SetViewports(ViewportF[] viewports)
        {
            throw new NotImplementedException();
        }

        public override void GetViewports(ViewportF[] outArray)
        {
            throw new NotImplementedException();
        }

        public override ViewportF GetViewport(int index)
        {
            throw new NotImplementedException();
        }

        public override void BeginEvent(string label)
        {
            throw new NotImplementedException();
        }

        public override void EndEvent()
        {
            throw new NotImplementedException();
        }

        public override void SetMarker(string label)
        {
            throw new NotImplementedException();
        }

        protected override ResourceMap GetResourcePtr(GraphicsResource resource, uint subresource, GraphicsMapType mapType)
        {
            throw new NotImplementedException();
        }

        protected override void OnUnmapResource(GraphicsResource resource, uint subresource)
        {
            throw new NotImplementedException();
        }

        protected override unsafe void UpdateResource(GraphicsResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch)
        {
            throw new NotImplementedException();
        }

        protected override void CopyResource(GraphicsResource src, GraphicsResource dest)
        {
            throw new NotImplementedException();
        }

        public override unsafe void CopyResourceRegion(GraphicsResource source, uint srcSubresource, ResourceRegion* sourceRegion, GraphicsResource dest, uint destSubresource, Vector3UI destStart)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _ptr);
        }

        internal DeviceDX12 Device { get; }

        internal Logger Log { get; }
        protected override GraphicsCommandList Cmd { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
