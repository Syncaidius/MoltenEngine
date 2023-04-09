using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphicsFrameTracker
    {
        const int INITIAL_BRANCH_COUNT = 3;

        public class TrackedFrame
        {
            public GraphicsCommandList[] Branches;

            public GraphicsFence Fence;

            public uint BranchCount { get; set; }
        }

        TrackedFrame[] _frames;
        uint _frameCount;
        uint _frameID;
        uint _frameIndex;

        internal GraphicsFrameTracker(GraphicsQueue queue)
        {
            Queue = queue;
            SetFrameCount((uint)Queue.Device.Settings.BufferingMode.Value);
            Queue.Device.Settings.BufferingMode.OnChanged += BufferingMode_OnChanged;
        }

        private void BufferingMode_OnChanged(BackBufferMode oldValue, BackBufferMode newValue)
        {
            SetFrameCount((uint)Queue.Device.Settings.BufferingMode.Value);
        }

        private void SetFrameCount(uint frameCount)
        {
            if (frameCount != _frameCount)
            {
                _frameCount = frameCount;
                if (_frames == null || _frames.Length < _frameCount)
                    _frames = new TrackedFrame[_frameCount];
            }
        }

        internal void StartFrame()
        {
            if (_frameID == Queue.Device.Renderer.Profiler.FrameID)
                throw new InvalidOperationException("Cannot start a new frame before the previous frame has completed.");

            _frameID = Queue.Device.Renderer.Profiler.FrameID;
            _frameIndex = (_frameIndex + 1U) % _frameCount;

            _frames[_frameIndex].Fence?.Wait();
        }

        public void Track(GraphicsCommandList cmd)
        {
            TrackedFrame f = _frames[_frameIndex];

            if (cmd.BranchIndex == f.Branches.Length)
                Array.Resize(ref f.Branches, f.Branches.Length + INITIAL_BRANCH_COUNT);

            GraphicsCommandList last = f.Branches[cmd.BranchIndex];
            cmd.Previous = last;
            f.Branches[cmd.BranchIndex] = cmd;
        }

        internal void Reset()
        {
            foreach (TrackedFrame f in _frames)
            {
                for (int j = 0; j < f.Branches.Length; j++)
                {
                    if (f.Branches[j] == null)
                        continue;

                    f.Branches[j].Free();
                    f.Branches[j] = null;
                }
            }
        }

        internal GraphicsQueue Queue { get; }

        public TrackedFrame Frame => _frames[_frameIndex];
    }
}
