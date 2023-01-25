using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Logging;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    internal unsafe class CommandQueueDX12 : GraphicsCommandQueue
    {
        CommandQueueDesc _desc;
        ID3D12CommandQueue* _ptr;

        internal CommandQueueDX12(Logger log, DeviceDX12 device, DeviceBuilderDX12 builder, ref CommandQueueDesc desc) : 
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
            HResult r = Device.Ptr->CreateCommandQueue(ref _desc, &cmdGuid, &cmdQueue);
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

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _ptr);
        }

        public override GraphicsBindResult Draw(IMaterial material, uint vertexCount, VertexTopology topology, uint vertexStartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult DrawInstanced(IMaterial material, uint vertexCountPerInstance, uint instanceCount, VertexTopology topology, uint vertexStartIndex = 0, uint instanceStartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult DrawIndexed(IMaterial material, uint indexCount, VertexTopology topology, int vertexIndexOffset = 0, uint startIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBindResult DrawIndexedInstanced(IMaterial material, uint indexCountPerInstance, uint instanceCount, VertexTopology topology, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch(IComputeTask task, uint groupsX, uint groupsY, uint groupsZ)
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

        internal DeviceDX12 Device { get; }

        internal Logger Log { get; }
    }
}
