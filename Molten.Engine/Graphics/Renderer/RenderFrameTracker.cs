using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RenderFrameTracker : IDisposable
    {
        public delegate void FrameBufferSizeChangedHandler(uint oldSize, uint newSize);

        public event FrameBufferSizeChangedHandler OnFrameBufferSizeChanged;

        const int INITIAL_BRANCH_COUNT = 3;

        public class TrackedFrame
        {
            /// <summary>
            /// Gets the <see cref="GraphicsBuffer"/> used for staging during the current frame.
            /// </summary>
            public GraphicsBuffer StagingBuffer { get; internal set; }

            /// <summary>
            /// Gets the fence to wait for at the end of the current frame or start of the next one.
            /// </summary>
            public GraphicsFence Fence;

            /// <summary>
            /// Gets the internal frame ID.
            /// </summary>
            internal ulong FrameID;

            GraphicsCommandList[] _branches;

            internal TrackedFrame()
            {
                _branches = new GraphicsCommandList[INITIAL_BRANCH_COUNT];
            }

            public void Track(GraphicsCommandList cmd)
            {
                if (cmd.BranchIndex == _branches.Length)
                    Array.Resize(ref _branches, _branches.Length + INITIAL_BRANCH_COUNT);

                GraphicsCommandList last = _branches[cmd.BranchIndex];
                cmd.Previous = last;
                _branches[cmd.BranchIndex] = cmd;
            }

            internal void Reset()
            {
                FrameID = 0;
                Fence = null;

                // Free all command lists in the frame.
                for (int i = 0; i < _branches.Length; i++)
                {
                    GraphicsCommandList q = _branches[i];
                    while (q != null)
                    {
                        GraphicsCommandList prev = q.Previous;
                        q.Free();
                        q = prev;
                    }
                }

                Array.Clear(_branches);
            }

            internal void Dispose()
            {
                Reset();
                StagingBuffer?.Dispose();
                StagingBuffer = null;
            }

            public uint BranchCount { get; set; }

            public GraphicsCommandList this[uint index] => _branches[index];
        }

        TrackedFrame[] _frames;
        uint _frameIndex;
        uint _newFrameBufferSize;
        uint _maxStagingSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer">The <see cref="RenderService"/> to bind the tracker to.</param>
        /// <param name="maxStagingSizeMB">The maximum size of a frame staging buffer, in megabytes.</param>
        internal RenderFrameTracker(RenderService renderer, double maxStagingSizeMB)
        {
            _maxStagingSize = (uint)ByteMath.FromMegabytes(maxStagingSizeMB);

            SettingValue<BackBufferMode> bufferingMode = renderer.Settings.Graphics.BufferingMode;
            _newFrameBufferSize = Math.Max(1, (uint)bufferingMode.Value);
            Queue = renderer.Device.Queue;
            bufferingMode.OnChanged += BufferingMode_OnChanged;
            StartFrame();
        }

        private void BufferingMode_OnChanged(BackBufferMode oldValue, BackBufferMode newValue)
        {
            _newFrameBufferSize = Math.Max(1, (uint)newValue);
        }

        internal void StartFrame()
        {
            // Do we need to resize the number of buffered frames?
            if (_newFrameBufferSize != CurrentFrameBufferSize)
            {
                // Only trigger event if resizing and not initializing. CurrentFrameBufferSize is 0 when uninitialized.
                if (CurrentFrameBufferSize > 0)
                    OnFrameBufferSizeChanged?.Invoke(CurrentFrameBufferSize, _newFrameBufferSize);

                CurrentFrameBufferSize = _newFrameBufferSize;

                // Ensure we have enough staging buffers
                if (_frames == null || _frames.Length < CurrentFrameBufferSize)
                {
                    Array.Resize(ref _frames, (int)CurrentFrameBufferSize);
                    uint bufferBytes = _maxStagingSize;
                    for (int i = 0; i < _frames.Length; i++)
                    {
                        if (_frames[i] == null)
                        {
                            _frames[i] = new TrackedFrame()
                            {
                                StagingBuffer = Queue.Device.CreateStagingBuffer(true, true, bufferBytes),
                            };
                        }
                    }
                }
            }

            // If the oldest frame hasn't finished yet, wait for it before replacing it with a new one.
            // This stops the CPU from getting too far ahead of the GPU.
            _frames[_frameIndex].Fence?.Wait();
            _frames[_frameIndex].Reset();

            // Ensure we don't have too many tracked frames.
            // TODO Check how many full runs we've done and wait until we've done at least 2 before disposing of any tracked frames.
            //      Reset run count if buffer size is changed.
            if (_frames.Length > CurrentFrameBufferSize)
            {
                for (int i = _frames.Length; i < CurrentFrameBufferSize; i++)
                {
                    _frames[i].Dispose();
                    _frames[i] = null;
                }
            }
        }

        internal void EndFrame()
        {
            _frames[_frameIndex].FrameID = Queue.Device.Renderer.Profiler.FrameID;
            _frames[_frameIndex].Fence?.Reset();

            _frameIndex = (_frameIndex + 1U) % CurrentFrameBufferSize;
        }

        public void Dispose()
        {
            for (int i = 0; i < _frames.Length; i++)
                _frames[i].Dispose();
        }

        /// <summary>
        /// Gets the graphics queue that this frame tracker is associated with.
        /// </summary>
        internal GraphicsQueue Queue { get; }

        /// <summary>
        /// Gets the currently tracked frame.
        /// </summary>
        public TrackedFrame Frame => _frames[_frameIndex];

        /// <summary>
        /// Gets the current frame-buffer size. The value will be between 1 and <see cref="GraphicsSettings.BufferingMode"/>, from <see cref="GraphicsDevice.Settings"/>.
        /// </summary>
        public uint CurrentFrameBufferSize { get; private set; }

        /// <summary>
        /// Gets the current frame index. The value will be between 0 and <see cref="GraphicsSettings.BufferingMode"/> - 1, from <see cref="GraphicsDevice.Settings"/>.
        /// </summary>
        public uint BackBufferIndex => _frameIndex;

        /// <summary>
        /// Gets the maximum size of a frame's staging buffer, in bytes.
        /// </summary>
        public uint MaxStagingBufferSize => _maxStagingSize;
    }
}
