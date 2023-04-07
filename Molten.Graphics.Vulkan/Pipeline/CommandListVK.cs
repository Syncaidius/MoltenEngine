using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class CommandListVK : GraphicsCommandList
    {
        CommandPoolAllocation _allocation;
        CommandBuffer _cmdBuffer;
        Vk _vk;
        DeviceVK _device;

        internal CommandListVK(CommandPoolAllocation allocation, CommandBuffer cmdBuffer, GraphicsCommandListType type) : 
            base(allocation.Pool.Queue, type)
        {
            _allocation = allocation;
            _cmdBuffer = cmdBuffer;
            _device = allocation.Pool.Queue.VKDevice;
            _vk = allocation.Pool.Queue.VK;
        }

        internal void Free()
        {
            if (IsFree)
                return;

            IsFree = true;
            _allocation.Free(this);
        }

        public override void Begin(bool singleUse)
        {
            base.Begin();

            if (IsFree)
                throw new InvalidOperationException("Cannot use a freed command list");

            CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            if (singleUse)
                beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;

            _vk.BeginCommandBuffer(_cmdBuffer, &beginInfo);
        }

        public override void End()
        {
            base.End();
            _vk.EndCommandBuffer(_cmdBuffer);
        }

        protected override unsafe ResourceMap GetResourcePtr(GraphicsResource resource, uint subresource, GraphicsMapType mapType)
        {
            ResourceMap map = new ResourceMap(null, resource.SizeInBytes, resource.SizeInBytes); // TODO Calculate correct RowPitch value when mapping textures
            Result r = _vk.MapMemory(_device, (((ResourceHandleVK*)resource.Handle)->Memory), 0, resource.SizeInBytes, 0, &map.Ptr);

            if (!r.Check(_device))
                return new ResourceMap();

            return map;
        }

        protected override unsafe void OnUnmapResource(GraphicsResource resource, uint subresource)
        {
            _vk.UnmapMemory(_device, (((ResourceHandleVK*)resource.Handle)->Memory));
        }

        protected override unsafe void UpdateResource(GraphicsResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch)
        {
            throw new NotImplementedException();
        }

        protected override void CopyResource(GraphicsResource src, GraphicsResource dest)
        {
            /*switch (src.ResourceType) {
                case GraphicsResourceType.Buffer:
                    _vk.CmdCopyBuffer(_cmdBuffer, *(Buffer*)src.Ptr, *(Buffer*)dest.Ptr);
                    break;

                case GraphicsResourceType.Texture:
                    // _vk.CmdCopyImage();
                    break;
            }*/
        }

        public override unsafe void CopyResourceRegion(GraphicsResource source, uint srcSubresource, ResourceRegion* sourceRegion, GraphicsResource dest, uint destSubresource, Vector3UI destStart)
        {
            
        }

        public override GraphicsBindResult Draw(HlslShader shader, uint vertexCount, uint vertexStartIndex = 0)
        {
            // TODO apply state

            _vk.CmdDraw(_cmdBuffer, vertexCount, 1, vertexStartIndex, 0);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult DrawInstanced(HlslShader shader, uint vertexCountPerInstance, uint instanceCount, uint vertexStartIndex = 0, uint instanceStartIndex = 0)
        {

            _vk.CmdDraw(_cmdBuffer, vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult DrawIndexed(HlslShader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
        {
            _vk.CmdDrawIndexed(_cmdBuffer, indexCount, 1, startIndex, vertexIndexOffset, 0);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult DrawIndexedInstanced(HlslShader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
        {
            _vk.CmdDrawIndexed(_cmdBuffer, indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult Dispatch(HlslShader shader, Vector3UI groups)
        {
            _vk.CmdDispatch(_cmdBuffer, groups.X, groups.Y, groups.Z);
            return GraphicsBindResult.Successful;
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }

        // TODO implement command buffer commands - CmdDraw, CmdCopyBuffer, etc

        internal bool IsFree { get; set; }

        internal CommandBuffer Native => _cmdBuffer;

        internal CommandBufferLevel Level => _allocation.Level;
    }
}
