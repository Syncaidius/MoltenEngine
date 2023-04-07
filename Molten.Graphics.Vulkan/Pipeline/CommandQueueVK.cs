using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class CommandQueueVK : GraphicsCommandQueue
    {
        DeviceVK _device;
        CommandPoolVK _poolFrame;
        CommandPoolVK _poolTransient;

        uint _cmdFrameID;
        CommandListVK[] _cmdFrame;

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
            int numBuffers = (int)Device.Settings.BufferingMode.Value;
            if (_cmdFrame.Length < numBuffers)
            {
                Array.Resize(ref _cmdFrame, numBuffers);
                for (int i = 0; i < numBuffers; i++)
                    _cmdFrame[i] = _cmdFrame[i] ?? _poolFrame.Allocate(CommandBufferLevel.Primary);
            }
            else
            {
                // Free excess buffers and downsize buffer array.
                for (int i = numBuffers; i < _cmdFrame.Length; i++)
                    _cmdFrame[i].Free();

                Array.Resize(ref _cmdFrame, numBuffers);
            }
        }

        public GraphicsCommandList StartFrame()
        {
            CommandListVK list = _cmdFrame[_cmdFrameID];

            // TODO if we hit a frame command list that isn't finished yet, we need to wait to prevent the CPU from getting too far ahead.
            // list.Fence.Wait(); -- A wait period of 0 should equate to "wait until done, no timeout", across all APIs.

            _cmdFrameID = (_cmdFrameID + 1U) % (uint)_cmdFrame.Length;
            return list;
        }

        public override GraphicsCommandList GetList(GraphicsCommandListType type)
        {
            CommandListVK cmd = null;

            switch (type)
            {
                case GraphicsCommandListType.Frame:
                    cmd = _poolFrame.Allocate(CommandBufferLevel.Secondary);
                    break;

                case GraphicsCommandListType.Short:
                    cmd = _poolTransient.Allocate(CommandBufferLevel.Secondary);
                    break;
            }

            return cmd;
        }

        public override unsafe FenceVK Submit(Action CompletionCallback, params GraphicsCommandList[] cmd)
        {
            FenceVK fence = _device.GetFence(CompletionCallback);
            SubmitCommandLists(cmd, fence.Ptr);
            return fence;
        }

        public override void Submit(params GraphicsCommandList[] cmd)
        {
            SubmitCommandLists(cmd, new Fence());
        }

        private unsafe void SubmitCommandLists(GraphicsCommandList[] cmd, Fence fence)
        {
            SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
            submit.CommandBufferCount = (uint)cmd.Length;

            CommandBuffer* ptrBuffers = stackalloc CommandBuffer[cmd.Length];
            for (int i = 0; i < cmd.Length; i++)
            {
                CommandListVK list = cmd[i] as CommandListVK;

                if (list.Level != CommandBufferLevel.Primary)
                    throw new InvalidOperationException($"Cannot submit a secondary command list directly to a command queue.");

                ptrBuffers[i] = list.Native;
            }

            submit.PCommandBuffers = ptrBuffers;
            Result r = VK.QueueSubmit(Native, 1, &submit, fence);
            r.Throw(_device, () => $"Failed to submit {cmd.Length} command lists");
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
