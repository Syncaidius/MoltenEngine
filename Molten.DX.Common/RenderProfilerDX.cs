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
        int _curShot;
        int _prevShot;

        int _curSecondShot;
        int _prevSecondShot;
        double _timing;
        Stopwatch _frameTimer;

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
            _curShot = 0;
            _prevShot = 0;
            _curSecondShot = 0;
            _prevSecondShot = 0;
            _timing = 0;
        }

        /// <summary>Adds a frame snapshot to the data for the current frame. Useful for collating snapshots from multiple device contexts into one snapshot.</summary>
        public void AddData(RenderFrameSnapshot snap)
        {
            _frameSnaps[_curShot].Add(snap);
        }

        public void StartCapture()
        {
            _frameTimer.Restart();
        }

        public void EndCapture(Timing time)
        {
            _frameTimer.Stop();

            // Accumulate into per-second. Reset for next frame.
            _frameSnaps[_curShot].Time = _frameTimer.Elapsed.TotalMilliseconds;
            _frameSnaps[_curShot].TargetTime = time.TargetFrameTime;
            _frameSnaps[_curShot].FrameID = FrameCount;
            _secondSnaps[_curSecondShot].Add(_frameSnaps[_curShot]);

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

            _prevShot = _curShot++;
            if (_curShot == _frameSnaps.Length)
                _curShot = 0;

            _frameSnaps[_curShot] = new RenderFrameSnapshot();
            FrameCount++;
        }

        public RenderFrameSnapshot PreviousFrame
        {
            get { return _frameSnaps[_prevShot]; }
        }

        public RenderFrameSnapshot CurrentFrame
        {
            get { return _frameSnaps[_curShot]; }
        }

        public RenderFrameSnapshot PreviousSecond
        {
            get { return _secondSnaps[_prevSecondShot]; }
        }

        public RenderFrameSnapshot CurrentSecond
        {
            get { return _secondSnaps[_curSecondShot]; }
        }

        /// <summary>Gets or sets the number of draw calls for the current frame.</summary>
        public int DrawCalls
        {
            get { return _frameSnaps[_curShot].DrawCalls; }
            set { _frameSnaps[_curShot].DrawCalls = value; }
        }

        /// <summary>Gets or sets the number of times a resource was bound to the device.</summary>
        public int Bindings
        {
            get { return _frameSnaps[_curShot].Bindings; }
            set { _frameSnaps[_curShot].Bindings = value; }
        }

        /// <summary>Gets or sets the number of render target swaps for the current frame.</summary>
        public int RTSwaps
        {
            get { return _frameSnaps[_curShot].RTSwaps; }
            set { _frameSnaps[_curShot].RTSwaps = value; }
        }

        /// <summary>Gets or sets the triangle count for the current frame.</summary>
        public int TriangleCount
        {
            get { return _frameSnaps[_curShot].TriCount; }
            set { _frameSnaps[_curShot].TriCount = value; }
        }

        /// <summary>Gets or sets the number of buffer swaps during the current frame.</summary>
        public int BufferSwaps
        {
            get { return _frameSnaps[_curShot].BufferSwaps; }
            set { _frameSnaps[_curShot].BufferSwaps = value; }
        }

        public int ShaderSwaps
        {
            get { return _frameSnaps[_curShot].ShaderSwaps; }
            set { _frameSnaps[_curShot].ShaderSwaps = value; }
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
