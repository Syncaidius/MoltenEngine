using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics.Vulkan
{
    internal class GraphicsQueueVK : GraphicsQueue
    {
        DeviceVK _device;
        Vk _vk;
        CommandPoolVK _poolFrame;
        CommandPoolVK _poolTransient;
        CommandListVK _cmd;

        Stack<DebugUtilsLabelEXT> _eventLabelStack;

        internal GraphicsQueueVK(RendererVK renderer, DeviceVK device, uint familyIndex, Queue queue, uint queueIndex, SupportedCommandSet set) :
            base(device)
        {
            _vk = renderer.VK;
            Log = renderer.Log;
            Flags = set.CapabilityFlags;
            _device = device;
            FamilyIndex = familyIndex;
            Index = queueIndex;
            Native = queue;
            Set = set;

            _eventLabelStack = new Stack<DebugUtilsLabelEXT>();
            _poolFrame = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit, 1);
            _poolTransient = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit | CommandPoolCreateFlags.TransientBit, 5);
        }

        public override unsafe void Begin(GraphicsCommandListFlags flags)
        {
            base.Begin();

            CommandBufferLevel level = flags.Has(GraphicsCommandListFlags.Deferred) ?
                CommandBufferLevel.Secondary :
                CommandBufferLevel.Primary;

            CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            beginInfo.Flags = CommandBufferUsageFlags.None;

            if(flags.Has(GraphicsCommandListFlags.SingleSubmit))
                beginInfo.Flags |= CommandBufferUsageFlags.OneTimeSubmitBit;

            _cmd = _poolFrame.Allocate(level, Device.Renderer.Frame.BranchCount++, flags);
            Device.Renderer.Frame.Track(_cmd);
            _vk.BeginCommandBuffer(_cmd, &beginInfo);
        }

        public override GraphicsCommandList End()
        {
            base.End();

            _vk.EndCommandBuffer(_cmd);
            return _cmd;
        }

        /// <inheritdoc/>
        public override unsafe void Execute(GraphicsCommandList list)
        {
            CommandListVK vkList = list as CommandListVK;
            if (vkList.Level != CommandBufferLevel.Secondary)
                throw new InvalidOperationException("Cannot submit a queue-level command list to a queue");

            CommandBuffer* cmdBuffers = stackalloc CommandBuffer[1] { vkList.Ptr };
            _vk.CmdExecuteCommands(_cmd, 1, cmdBuffers);
        }

        /// <inheritdoc/>
        public override unsafe void Submit(GraphicsCommandListFlags flags)
        {
            if (_cmd.Level != CommandBufferLevel.Primary)
                throw new InvalidOperationException($"Cannot submit a secondary command list directly to a command queue.");

            // Use empty fence handle if the CPU doesn't need to wait for the command list to finish.
            Fence fence = new Fence();
            if (_cmd.Fence != null)
                fence = (_cmd.Fence as FenceVK).Ptr;

            // We're only submitting the current command buffer.
            _vk.EndCommandBuffer(_cmd);
            CommandBuffer* ptrBuffers = stackalloc CommandBuffer[] { _cmd.Ptr };
            SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
            submit.PCommandBuffers = ptrBuffers;

            // We want to wait on the previous command list's semaphore before executing this one, if any.
            if(_cmd.Previous != null)
            {
                Semaphore* waitSemaphores = stackalloc Semaphore[] { (_cmd.Previous as CommandListVK).Semaphore.Ptr };
                submit.WaitSemaphoreCount = 1;
                submit.PWaitSemaphores = waitSemaphores;
            }
            else
            {
                submit.WaitSemaphoreCount = 0;
                submit.PWaitSemaphores = null;
            }

            // We want to signal the command list's own semaphore so that the next command list can wait on it, if needed.
            _cmd.Semaphore.Start(SemaphoreCreateFlags.None);
            Semaphore* semaphore = stackalloc Semaphore[] { _cmd.Semaphore.Ptr };
            submit.CommandBufferCount = 1;
            submit.SignalSemaphoreCount = 1;
            submit.PSignalSemaphores = semaphore;

            Result r = VK.QueueSubmit(Native, 1, &submit, fence);
            r.Throw(_device, () => "Failed to submit command list");

            // Allocate next command buffer
            if (!flags.Has(GraphicsCommandListFlags.Last))
            {
                _cmd = _poolFrame.Allocate(CommandBufferLevel.Primary, _cmd.BranchIndex, flags);
                CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
                beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;

                _vk.BeginCommandBuffer(_cmd, &beginInfo);
                Device.Renderer.Frame.Track(_cmd);
            }
        }

        /// <summary>
        /// Queues a texture memory barrier.
        /// </summary>
        /// <param name="srcFlags">The source stage flags.</param>
        /// <param name="destFlags">The destination stage flags.</param>
        /// <param name="barrier">A pointer to one or more image memory barriers.</param>
        /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
        internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, ImageMemoryBarrier* barrier, uint barrierCount = 1)
        {
            _vk.CmdPipelineBarrier(_cmd, srcFlags, destFlags, DependencyFlags.None, 0, null, 0, null, barrierCount, barrier);
        }

        /// <summary>
        /// Queues a buffer memory barrier.
        /// </summary>
        /// <param name="srcFlags">The source stage flags.</param>
        /// <param name="destFlags">The destination stage flags.</param>
        /// <param name="barrier">A pointer to one or more buffer memory barriers.</param>
        /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
        internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, BufferMemoryBarrier* barrier, uint barrierCount = 1)
        {
            _vk.CmdPipelineBarrier(_cmd, srcFlags, destFlags, DependencyFlags.None, 0, null, barrierCount, barrier, 0, null);
        }

        /// <summary>
        /// Queues a global memory barrier command.
        /// </summary>
        /// <param name="srcFlags">The source stage flags.</param>
        /// <param name="destFlags">The destination stage flags.</param>
        /// <param name="barrier">A pointer to one or more global memory barriers.</param>
        /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
        internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, MemoryBarrier* barrier, uint barrierCount = 1)
        {
            _vk.CmdPipelineBarrier(_cmd, srcFlags, destFlags, DependencyFlags.None, barrierCount, barrier, 0, null, 0, null);
        }

        internal unsafe void ClearImage(Image image, ImageLayout layout, Color color, ImageSubresourceRange* pRanges, uint numRanges)
        {
            _vk.CmdClearColorImage(_cmd, image, layout, *(ClearColorValue*)&color, numRanges, pRanges);
        }

        internal bool HasFlags(CommandSetCapabilityFlags flags)
        {
            return (Flags & flags) == flags;
        }

        protected override void OnDispose()
        {
            
            _poolFrame.Dispose();
            _poolTransient.Dispose();
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

        public override unsafe void BeginEvent(string label)
        {
            RendererVK renderer = Device.Renderer as RendererVK;

            byte* ptrString = EngineUtil.StringToPtr(label, Encoding.UTF8);
            DebugUtilsLabelEXT lbl = new DebugUtilsLabelEXT(pLabelName: ptrString);

            float* ptrColor = stackalloc float[] { 1f, 1f, 1f, 1f };
            NativeMemory.Copy(&ptrColor, lbl.Color, sizeof(float) * 4);

            renderer.DebugLayer.Module.CmdBeginDebugUtilsLabel(_cmd, &lbl);
            _eventLabelStack.Push(lbl);
        }

        public unsafe override void EndEvent()
        {
            DebugUtilsLabelEXT lbl = _eventLabelStack.Pop();
            EngineUtil.Free(ref lbl.PLabelName);
        }

        public override void SetMarker(string label)
        {
            BeginEvent(label);
            EndEvent();
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

        protected override unsafe void CopyResource(GraphicsResource src, GraphicsResource dest)
        {
            ResourceHandleVK* srcHandle = (ResourceHandleVK*)src.Handle;
            ResourceHandleVK* destHandle = (ResourceHandleVK*)dest.Handle;

            if (src is GraphicsBuffer)
            {
                Buffer* srcBuffer = srcHandle->As<Buffer>();

                if (dest is GraphicsBuffer)
                {
                    Buffer* destBuffer = destHandle->As<Buffer>();
                    Span<BufferCopy> copy = stackalloc BufferCopy[] {
                        new BufferCopy(0, 0, src.SizeInBytes)
                    };
                    _vk.CmdCopyBuffer(_cmd, *srcBuffer, *destBuffer, copy);
                }
                else if (dest is GraphicsTexture destTex)
                {
                    Image* destImg = destHandle->As<Image>();
                    Offset3D offset = new Offset3D(0, 0, 0);
                    Extent3D extent = new Extent3D(destTex.Width, destTex.Height, destTex.Depth);

                    // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.

                    ImageSubresourceLayers layers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                    Span<BufferImageCopy> regions = stackalloc BufferImageCopy[] {
                        new BufferImageCopy(0, 0, src.SizeInBytes, layers, offset, extent)
                    };

                    _vk.CmdCopyBufferToImage(_cmd, *srcBuffer, *destImg, ImageLayout.TransferDstOptimal, regions);
                }
            }
            else if (src is GraphicsTexture srcTex)
            {
                Image* srcImg = srcHandle->As<Image>();
                Offset3D srcOffset = new Offset3D(0, 0, 0);

                // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.
                ImageSubresourceLayers srcLayers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                Extent3D srcExtent = new Extent3D(srcTex.Width, srcTex.Height, srcTex.Depth);

                if (dest is GraphicsBuffer)
                {
                    Buffer* destBuffer = destHandle->As<Buffer>();
                    Span<BufferImageCopy> regions = stackalloc BufferImageCopy[] {
                        new BufferImageCopy(0, 0, src.SizeInBytes, srcLayers, srcOffset, srcExtent)
                    };

                    _vk.CmdCopyImageToBuffer(_cmd, *srcImg, ImageLayout.TransferSrcOptimal, *destBuffer, regions);
                }
                else if (dest is GraphicsTexture destTex)
                {
                    Image* destImg = destHandle->As<Image>();
                    Offset3D destOffset = new Offset3D(0, 0, 0);

                    // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.

                    ImageSubresourceLayers destLayers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                    Span<ImageCopy> regions = stackalloc ImageCopy[] {
                        new ImageCopy(srcLayers, srcOffset, destLayers, destOffset, srcExtent),
                    };

                    _vk.CmdCopyImage(_cmd, *srcImg, ImageLayout.TransferSrcOptimal, *destImg, ImageLayout.TransferDstOptimal, regions);
                }
            }
            else
            {
                throw new InvalidOperationException($"Unsupported copy resource type '{src.GetType().Name}'");
            }
        }

        public override unsafe void CopyResourceRegion(GraphicsResource source, uint srcSubresource, ResourceRegion* sourceRegion, GraphicsResource dest, uint destSubresource, Vector3UI destStart)
        {

        }

        public override GraphicsBindResult Draw(HlslShader shader, uint vertexCount, uint vertexStartIndex = 0)
        {
            // TODO apply state

            _vk.CmdDraw(_cmd, vertexCount, 1, vertexStartIndex, 0);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult DrawInstanced(HlslShader shader, uint vertexCountPerInstance, uint instanceCount, uint vertexStartIndex = 0, uint instanceStartIndex = 0)
        {
            // TODO apply state

            _vk.CmdDraw(_cmd, vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult DrawIndexed(HlslShader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
        {
            // TODO apply state

            _vk.CmdDrawIndexed(_cmd, indexCount, 1, startIndex, vertexIndexOffset, 0);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult DrawIndexedInstanced(HlslShader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
        {
            // TODO apply state

            _vk.CmdDrawIndexed(_cmd, indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult Dispatch(HlslShader shader, Vector3UI groups)
        {
            // TODO apply state

            _vk.CmdDispatch(_cmd, groups.X, groups.Y, groups.Z);
            return GraphicsBindResult.Successful;
        }

        internal Vk VK => _vk;

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

        /// <summary>
        /// Gets flags representing the available API command sets.
        /// </summary>
        internal CommandSetCapabilityFlags Flags { get; }

        internal Queue Native { get; private set; }

        /// <summary>
        /// The current command list, if any.
        /// </summary>
        protected override GraphicsCommandList Cmd
        {
            get => _cmd;
            set => _cmd = value as CommandListVK;
        }
    }
}
