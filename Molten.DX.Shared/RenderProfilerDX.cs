using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RenderProfilerDX : IRenderProfiler
    {
        RenderFrameSnapshot[] _frameSnaps;
        RenderFrameSnapshot[] _secondSnaps;
        int _nextSlot;
        int _prevSlot;

        int _curSecondShot;
        int _prevSecondShot;
        double _timing;
        Stopwatch _frameTimer;
        public RenderFrameSnapshot CurrentFrame;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snapshots">The number of previous frames of which to keep snapshots.</param>
        public RenderProfilerDX(int snapshots = 20)
        {
            _frameTimer = new Stopwatch();
            _frameSnaps = new RenderFrameSnapshot[snapshots];
            _secondSnaps = new RenderFrameSnapshot[snapshots];
        }

        public void Clear()
        {
            Array.Clear(_frameSnaps, 0, _frameSnaps.Length);
            Array.Clear(_secondSnaps, 0, _secondSnaps.Length);
            _nextSlot = 0;
            _prevSlot = 0;
            _curSecondShot = 0;
            _prevSecondShot = 0;
            _timing = 0;
        }

        /// <summary>Adds a frame snapshot to the data for the current frame. Useful for collating snapshots from multiple device contexts into one snapshot.</summary>
        public void AddData(RenderFrameSnapshot snap)
        {
            CurrentFrame.Add(snap);
        }

        public void StartCapture()
        {
            _frameTimer.Restart();
        }

        public void EndCapture(Timing time)
        {
            _frameTimer.Stop();

            // Accumulate into per-second. Reset for next frame.
            CurrentFrame.Time = _frameTimer.Elapsed.TotalMilliseconds;
            CurrentFrame.TargetTime = time.TargetFrameTime;
            CurrentFrame.FrameID = FrameCount;
            _frameSnaps[_nextSlot] = CurrentFrame;
            _secondSnaps[_curSecondShot].Add(CurrentFrame);

            // Handle per-second timing updates.
            _timing += time.ElapsedTime.TotalMilliseconds;
            if (_timing >= 1000)
            {
                _timing -= 1000;
                _prevSecondShot = _curSecondShot;
                _curSecondShot++;

                if (_curSecondShot == _secondSnaps.Length)
                    _curSecondShot = 0;

                _secondSnaps[_curSecondShot] = new RenderFrameSnapshot();
            }

            _prevSlot = _nextSlot++;
            if (_nextSlot == _frameSnaps.Length)
                _nextSlot = 0;

            CurrentFrame = new RenderFrameSnapshot();
            FrameCount++;
        }

        public RenderFrameSnapshot PreviousFrame
        {
            get { return _frameSnaps[_prevSlot]; }
        }

        RenderFrameSnapshot IRenderProfiler.CurrentFrame
        {
            get { return CurrentFrame; }
        }

        public RenderFrameSnapshot PreviousSecond
        {
            get { return _secondSnaps[_prevSecondShot]; }
        }

        public RenderFrameSnapshot CurrentSecond
        {
            get { return _secondSnaps[_curSecondShot]; }
        }

        /// <summary>Gets the number of draw calls for the current second.</summary>
        public int DrawCallsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].DrawCalls; }
        }

        /// <summary>Gets the number of texture swaps for the current second.</summary>
        public int BindingsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].Bindings; }
        }

        /// <summary>Gets the number of render target swaps for the current second.</summary>
        public int RTSwapsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].RTSwaps; }
        }

        /// <summary>Gets the triangle count for the current second.</summary>
        public int TriangleCountPerSecon
        {
            get { return _secondSnaps[_curSecondShot].TriCount; }
        }

        public int BufferSwapsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].BufferSwaps; }
        }

        public int ShaderSwapsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].ShaderSwaps; }
        }

        /// <summary>
        /// Gets the number of frames recorded with this profiler.
        /// </summary>
        public ulong FrameCount { get; private set; }
    }
}
