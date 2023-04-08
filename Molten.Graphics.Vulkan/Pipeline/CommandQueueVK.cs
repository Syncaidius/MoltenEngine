using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics
{
    internal class CommandQueueVK : GraphicsCommandQueue
    {
        DeviceVK _device;
        CommandPoolVK _poolFrame;
        CommandPoolVK _poolTransient;

        uint _cmdIndex;
        FrameTrackerVK[] _frames;
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
            if (_frames.Length < _frameCount)
            {
                Array.Resize(ref _frames, _frameCount);
                for (int i = 0; i < _frameCount; i++)
                    _frames[i] = _frames[i] ?? new FrameTrackerVK(_device);
            }
        }

        internal void StartFrame()
        {
            if (_frameID == _device.Renderer.Profiler.FrameID)
                throw new InvalidOperationException("Cannot start a new frame before the previous frame has completed.");

            _frameID = _device.Renderer.Profiler.FrameID;

            // TODO if we hit a frame command list that isn't finished yet, we need to wait to prevent the CPU from getting too far ahead.
            // FrameTrackerVK.Wait() needs to be added to wait for the last fence of each branch to finish.

            _cmdIndex = (_cmdIndex + 1U) % (uint)_frames.Length;
        }

        /// <summary>
        /// Starts a new branch of command lists for the current frame. The command lists of each branch are kept sequentially in sync via semaphores.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public GraphicsCommandList StartBranch(GraphicsCommandListFlags flags)
        {
            FrameTrackerVK tracker = _frames[_cmdIndex];
            CommandListVK list = _poolFrame.Allocate(CommandBufferLevel.Primary);
            list.Index = 0;
            list.BranchIndex = tracker.BranchCount;
            tracker.Track(list);
            tracker.BranchCount++;



            return list;
        }

        public unsafe GraphicsCommandList Submit(GraphicsCommandList cmd, GraphicsCommandListFlags flags)
        {
            CommandListVK vkCmd = cmd as CommandListVK;

            if (vkCmd.Level != CommandBufferLevel.Primary)
                throw new InvalidOperationException($"Cannot submit a secondary command list directly to a command queue.");

            Fence f = new Fence();
            if(flags.Has(GraphicsCommandListFlags.CpuSyncable))
            {
                FenceVK fence = _device.GetFence();
                vkCmd.Fence = fence;
                f = fence.Ptr;
            }
            else
            {
                vkCmd.Fence = null;
            }

            vkCmd.Semaphore.Start(SemaphoreCreateFlags.None);
            Semaphore* semaphore = stackalloc Semaphore[] { vkCmd.Semaphore.Ptr };
            SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
            submit.CommandBufferCount = 1;
            submit.PSignalSemaphores = semaphore;
            submit.SignalSemaphoreCount = 1;

            CommandBuffer* ptrBuffers = stackalloc CommandBuffer[] { vkCmd.Native };
            submit.PCommandBuffers = ptrBuffers;

            Result r = VK.QueueSubmit(Native, 1, &submit, f);
            r.Throw(_device, () => "Failed to submit command list");

            // TODO allocate next command buffer
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
