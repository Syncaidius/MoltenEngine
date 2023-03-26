using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics
{
    internal unsafe class CommandListVK
    {
        CommandPoolAllocation _allocation;
        CommandBuffer _cmdBuffer;
        Vk _vk;

        internal CommandListVK(CommandPoolAllocation allocation, CommandBuffer cmdBuffer)
        {
            _allocation = allocation;
            _cmdBuffer = cmdBuffer;
            _vk = allocation.Pool.Device.VK;
        }

        internal void Free()
        {
            if (IsFree)
                return;

            IsFree = true;
            _allocation.Free(this);
        }

        internal unsafe void Begin()
        {
            if (IsFree)
                throw new InvalidOperationException("Cannot use a freed command list");

            if (HasBegun)
                throw new InvalidOperationException("Cannot call Begin() again before End() has been called");

            HasBegun = true;

            CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            if (_allocation.Pool.IsTransient)
                beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;

            _vk.BeginCommandBuffer(_cmdBuffer, &beginInfo);
        }

        internal void End()
        {
            if (!HasBegun)
                throw new InvalidOperationException("Cannot call End() before Begin() has been called");

            _vk.EndCommandBuffer(_cmdBuffer);
            HasBegun = false;
        }

        internal unsafe void Submit()
        {
            SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
            submit.CommandBufferCount = 1;

            CommandBuffer* ptrBuffer = stackalloc CommandBuffer[] { _cmdBuffer };
            submit.PCommandBuffers = ptrBuffer;
        }

        internal void CopyResource(GraphicsResource src, GraphicsResource dest)
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

        // TODO implement command buffer commands - CmdDraw, CmdCopyBuffer, etc

        internal bool IsFree { get; set; }

        internal bool HasBegun { get; private set; }
    }
}
