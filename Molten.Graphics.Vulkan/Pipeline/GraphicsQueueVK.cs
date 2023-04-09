using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics
{
    internal class GraphicsQueueVK : GraphicsQueue
    {
        DeviceVK _device;
        Vk _vk;
        CommandPoolVK _poolFrame;
        CommandPoolVK _poolTransient;

        uint _trackerIndex;
        FrameTrackerVK[] _trackers;
        int _frameCount;
        ulong _frameID;

        CommandListVK _cmd;

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

            _poolFrame = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit, 1);
            _poolTransient = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit | CommandPoolCreateFlags.TransientBit, 5);

            InitFrameLists();
            Device.Settings.BufferingMode.OnChanged += BufferingMode_OnChanged;
        }

        private void BufferingMode_OnChanged(BackBufferMode oldValue, BackBufferMode newValue)
        {
            InitFrameLists();
        }

        private void InitFrameLists()
        {
            _frameCount = (int)Device.Settings.BufferingMode.Value;
            if (_trackers.Length < _frameCount)
            {
                Array.Resize(ref _trackers, _frameCount);
                for (int i = 0; i < _frameCount; i++)
                    _trackers[i] = _trackers[i] ?? new FrameTrackerVK();
            }
        }

        internal void StartFrame()
        {
            if (_frameID == _device.Renderer.Profiler.FrameID)
                throw new InvalidOperationException("Cannot start a new frame before the previous frame has completed.");

            _frameID = _device.Renderer.Profiler.FrameID;

            // TODO if we hit a frame command list that isn't finished yet, we need to wait to prevent the CPU from getting too far ahead.
            // FrameTrackerVK.Wait() needs to be added to wait for the last fence of each branch to finish.

            _trackerIndex = (_trackerIndex + 1U) % (uint)_trackers.Length;
        }

        public override unsafe void Begin(GraphicsCommandListFlags flags = GraphicsCommandListFlags.None)
        {
            base.Begin(flags);

            CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;

            FrameTrackerVK tracker = _trackers[_trackerIndex];
            _cmd = _poolFrame.Allocate(CommandBufferLevel.Primary, 0, tracker.BranchCount, flags);
            tracker.BranchCount++;
            tracker.Track(_cmd);

            _vk.BeginCommandBuffer(_cmd, &beginInfo);
        }

        public override void End()
        {
            base.End();
            _vk.EndCommandBuffer(_cmd);
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
            CommandBuffer* ptrBuffers = stackalloc CommandBuffer[] { _cmd.Ptr };
            SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
            submit.PCommandBuffers = ptrBuffers;

            // We want to wait on the previous command list's semaphore before executing this one, if any.
            CommandListVK prev = _trackers[_trackerIndex].GetPrevious(_cmd);
            if(prev != null)
            {
                Semaphore* waitSemaphores = stackalloc Semaphore[] { prev.Semaphore.Ptr };
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
            FrameTrackerVK tracker = _trackers[_trackerIndex];
            _cmd = _poolFrame.Allocate(CommandBufferLevel.Primary, _cmd.Index + 1, _cmd.BranchIndex, flags);
            tracker.Track(_cmd);
        }

        public override GraphicsCommandList Record(Action<GraphicsQueue> callback, GraphicsCommandListFlags flags)
        {
            CommandListVK tmpCmd = _cmd;
            FrameTrackerVK tmpTracker = _trackers[_trackerIndex];
            _trackers[_trackerIndex] = new FrameTrackerVK();
            Begin(flags);
            callback.Invoke(this);
            End();

            CommandListVK result = _cmd;
            _cmd = tmpCmd;
            _trackers[_trackerIndex] = tmpTracker;
            return result;
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
            switch (src) {
                case GraphicsBuffer buffer:
                    Span<BufferCopy> copy = stackalloc BufferCopy[] { new BufferCopy(0, 0, src.SizeInBytes) };
                    _vk.CmdCopyBuffer(_cmd, *(Buffer*)src.Handle, *(Buffer*)dest.Handle, copy);
                    break;

                case GraphicsTexture tex:
                    // _vk.CmdCopyImage();
                    break;
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
    }
}
