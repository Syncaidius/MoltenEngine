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
        const double MAX_STAGING_BUFFER_MEGABYTES = 5.5;

        public delegate void FrameBufferSizeChangedHandler(uint oldSize, uint newSize);

        public event FrameBufferSizeChangedHandler OnFrameBufferSizeChanged;

        const int INITIAL_BRANCH_COUNT = 3;

        public class TrackedFrame
        {
            public GraphicsCommandList[] Branches;

            public GraphicsBuffer StagingBuffer { get; internal set; }

            public GraphicsFence Fence;

            internal ulong FrameID;

            public void Track(GraphicsCommandList cmd)
            {
                if (cmd.BranchIndex == Branches.Length)
                    Array.Resize(ref Branches, Branches.Length + INITIAL_BRANCH_COUNT);

                GraphicsCommandList last = Branches[cmd.BranchIndex];
                cmd.Previous = last;
                Branches[cmd.BranchIndex] = cmd;
            }

            internal void Reset()
            {
                FrameID = 0;
                Fence = null;

                // Free all command lists in the frame.
                for (int i = 0; i < Branches.Length; i++)
                {
                    GraphicsCommandList q = Branches[i];
                    while (q != null)
                    {
                        GraphicsCommandList prev = q.Previous;
                        q.Free();
                        q = prev;
                    }
                }

                Array.Clear(Branches, 0, Branches.Length);
            }

            internal void Dispose()
            {
                Reset();
                StagingBuffer?.Dispose();
                StagingBuffer = null;
            }

            public uint BranchCount { get; set; }
        }

        TrackedFrame[] _frames;
        uint _frameIndex;
        uint _newFrameBufferSize;

        internal RenderFrameTracker(RenderService renderer)
        {
            _newFrameBufferSize = 1;
            Queue = renderer.Device.Queue;
            renderer.Settings.Graphics.BufferingMode.OnChanged += BufferingMode_OnChanged;
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
                    uint bufferBytes = (uint)ByteMath.FromMegabytes(MAX_STAGING_BUFFER_MEGABYTES);
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

                // If the oldest frame hasn't finished yet, wait for it before replacing it with a new one.
                // This stops the CPU from getting too far ahead of the GPU.
                _frames[_frameIndex].Fence?.Wait();

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

        internal GraphicsQueue Queue { get; }

        public TrackedFrame Frame => _frames[_frameIndex];

        public uint CurrentFrameBufferSize { get; private set; }

        /// <summary>
        /// Gets the current frame index. The value will be between 0 and <see cref="GraphicsSettings.BufferingMode"/> - 1, from <see cref="GraphicsDevice.Settings"/>.
        /// </summary>
        public uint FrameIndex => _frameIndex;
    }
}
