using Molten.IO;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class CommandQueueVK : GraphicsCommandQueue
    {
        DeviceVK _device;
        CommandPoolVK _cmdPool;
        CommandPoolVK _cmdTransientPool;

        CommandListVK _cmdMain;

        internal CommandQueueVK(RendererVK renderer, DeviceVK device, uint familyIndex, Queue queue, uint queueIndex, SupportedCommandSet set) :
            base(device)
        {
            VK = renderer.VK;
            Log = renderer.Log;
            Flags = set.CapabilityFlags;
            _device = device;
            Index = queueIndex;
            Native = queue;
            Set = set;

            _cmdPool = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit, 1);
            _cmdTransientPool = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit | CommandPoolCreateFlags.TransientBit, 5);

            _cmdMain = _cmdPool.Allocate(CommandBufferLevel.Primary);
        }

        internal bool HasFlags(CommandSetCapabilityFlags flags)
        {
            return (Flags & flags) == flags;
        }

        protected override void OnDispose()
        {
            _cmdMain.Free();
            _cmdPool.Dispose();
            _cmdTransientPool.Dispose();
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

        internal unsafe bool MapResource(GraphicsResource resource, uint subresource, uint streamOffset, out RawStream stream)
        {
            ResourceVK res = resource as ResourceVK;

            if (res.MapPtr != null)
                throw new InvalidOperationException($"Cannot map resource memory that is already mapped. Call UnmapResource() first");

            stream = null;
            void* ptr = null;
            Result r = VK.MapMemory(_device, *res.Memory, 0, res.SizeInBytes, 0, &ptr);
            if (!r.Check(_device))
                return false;

            res.MapPtr = ptr;
            bool canWrite = res.Flags.Has(GraphicsResourceFlags.CpuWrite);
            bool canRead = res.Flags.Has(GraphicsResourceFlags.CpuRead);
            stream = new RawStream(ptr, res.SizeInBytes, canRead, canWrite);
            stream.Position = streamOffset;
            return true;
        }

        internal unsafe void UnmapResource(GraphicsResource resource, uint subresource)
        {
            ResourceVK res = resource as ResourceVK;
            if (res.MapPtr == null)
                throw new InvalidOperationException($"Cannot unmap resource memory that is not mapped. Call MapResource() first");

            VK.UnmapMemory(_device, *res.Memory);
            res.MapPtr = null;
        }

        internal Vk VK { get; }

        internal Logger Log { get; }

        internal DeviceVK VKDevice => _device;

        /// <summary>
        /// Gets the Queue family index, in relation to the bound <see cref="DeviceVK"/>.
        /// </summary>
        internal uint FamilyIndex { get; }

        /// <summary>
        /// Gets the command queue index, within its family.
        /// </summary>
        internal uint Index { get; }

        /// <summary>
        /// Gets the underlying command set definition.
        /// </summary>
        internal SupportedCommandSet Set { get; }

        internal CommandSetCapabilityFlags Flags { get; }

        internal Queue Native { get; private set; }
    }
}
