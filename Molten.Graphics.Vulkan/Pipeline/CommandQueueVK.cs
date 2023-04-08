using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics
{
    internal class CommandQueueVK : GraphicsCommandQueue
    {
        DeviceVK _device;
        CommandPoolVK _poolFrame;
        CommandPoolVK _poolTransient;

        uint _trackerIndex;
        FrameTrackerVK[] _trackers;
        int _frameCount;
        ulong _frameID;

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
                    _trackers[i] = _trackers[i] ?? new FrameTrackerVK(_device);
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

        /// <summary>
        /// Starts a new branch of command lists for the current frame. The command lists of each branch are kept sequentially in sync via semaphores.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public GraphicsCommandList StartBranch(GraphicsCommandListFlags flags)
        {
            FrameTrackerVK tracker = _trackers[_trackerIndex];
            CommandListVK list = _poolFrame.Allocate(CommandBufferLevel.Primary, 0, tracker.BranchCount, flags);
            tracker.BranchCount++;
            tracker.Track(list);

            return list;
        }

        public unsafe GraphicsCommandList Submit(GraphicsCommandList cmd, GraphicsCommandListFlags flags)
        {
            CommandListVK vkCmd = cmd as CommandListVK;

            if (vkCmd.Level != CommandBufferLevel.Primary)
                throw new InvalidOperationException($"Cannot submit a secondary command list directly to a command queue.");

            // Use empty fence handle if the CPU doesn't need to wait for the command list to finish.
            Fence fence = new Fence();
            if (cmd.Fence != null)
                fence = (cmd.Fence as FenceVK).Ptr;            

            // We're only submitting the current command buffer.
            CommandBuffer* ptrBuffers = stackalloc CommandBuffer[] { vkCmd.Native };
            SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
            submit.PCommandBuffers = ptrBuffers;

            // We want to wait on the previous command list's semaphore before executing this one, if any.
            CommandListVK prev = _trackers[_trackerIndex].GetPrevious(vkCmd);
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
            vkCmd.Semaphore.Start(SemaphoreCreateFlags.None);
            Semaphore* semaphore = stackalloc Semaphore[] { vkCmd.Semaphore.Ptr };
            submit.CommandBufferCount = 1;
            submit.SignalSemaphoreCount = 1;
            submit.PSignalSemaphores = semaphore;

            Result r = VK.QueueSubmit(Native, 1, &submit, fence);
            r.Throw(_device, () => "Failed to submit command list");

            // Allocate next command buffer
            FrameTrackerVK tracker = _trackers[_trackerIndex];
            CommandListVK cmdNext = _poolFrame.Allocate(CommandBufferLevel.Primary, vkCmd.Index + 1, vkCmd.BranchIndex, flags);
            tracker.Track(cmdNext);

            return cmdNext;
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

        /// <summary>
        /// Gets flags representing the available API command sets.
        /// </summary>
        internal CommandSetCapabilityFlags Flags { get; }

        internal Queue Native { get; private set; }
    }
}
