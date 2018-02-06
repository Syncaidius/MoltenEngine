using System;
using System.Collections.Generic;
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

        long _vramUsed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snapshots">The number of previous frames of which to keep snapshots.</param>
        public RenderProfilerDX(int snapshots = 20)
        {
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
            _vramUsed = 0;
            _timing = 0;
        }

        /// <summary>Track a VRAM allocation.</summary>
        /// <param name="bytes">The number of bytes that were allocated.</param>
        public void TrackAllocation(long bytes)
        {
            Interlocked.Add(ref _vramUsed, bytes);
        }

        /// <summary>Track a VRAM deallocation.</summary>
        /// <param name="bytes">The number of bytes that were deallocated.</param>
        public void TrackDeallocation(long bytes)
        {
            Interlocked.Add(ref _vramUsed, -bytes);
        }

        /// <summary>Adds a frame snapshot to the data for the current frame. Useful for collating snapshots from multiple device contexts into one snapshot.</summary>
        public void AddData(ref RenderFrameSnapshot snap)
        {
            _frameSnaps[_curShot].Add(snap);
        }

        /// <summary>Stores the statistics for the current frame.</summary>
        public void CaptureFrame()
        {
            _prevShot = _curShot;
            _curShot++;

            if (_curShot == _frameSnaps.Length)
                _curShot = 0;

            // Accumulate into second-snapshot, then reset.
            _secondSnaps[_curSecondShot].Add(_frameSnaps[_curShot]);
            _frameSnaps[_curShot] = new RenderFrameSnapshot();
        }

        public void Capture(Timing time)
        {
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
            get { return _frameSnaps[_curShot].drawCalls; }
            set { _frameSnaps[_curShot].drawCalls = value; }
        }

        /// <summary>Gets or sets the number of times a resource was bound to the device.</summary>
        public int Bindings
        {
            get { return _frameSnaps[_curShot].bindings; }
            set { _frameSnaps[_curShot].bindings = value; }
        }

        /// <summary>Gets or sets the number of render target swaps for the current frame.</summary>
        public int RTSwaps
        {
            get { return _frameSnaps[_curShot].rtSwaps; }
            set { _frameSnaps[_curShot].rtSwaps = value; }
        }

        /// <summary>Gets or sets the triangle count for the current frame.</summary>
        public int TriangleCount
        {
            get { return _frameSnaps[_curShot].triCount; }
            set { _frameSnaps[_curShot].triCount = value; }
        }

        /// <summary>Gets or sets the number of buffer swaps during the current frame.</summary>
        public int BufferSwaps
        {
            get { return _frameSnaps[_curShot].bufferSwaps; }
            set { _frameSnaps[_curShot].bufferSwaps = value; }
        }

        public int ShaderSwaps
        {
            get { return _frameSnaps[_curShot].shaderSwaps; }
            set { _frameSnaps[_curShot].shaderSwaps = value; }
        }

        /// <summary>Gets the number of draw calls for the current second.</summary>
        public int DrawCallsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].drawCalls; }
        }

        /// <summary>Gets the number of texture swaps for the current second.</summary>
        public int BindingsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].bindings; }
        }

        /// <summary>Gets the number of render target swaps for the current second.</summary>
        public int RTSwapsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].rtSwaps; }
        }

        /// <summary>Gets the triangle count for the current second.</summary>
        public int TriangleCountPerSecon
        {
            get { return _secondSnaps[_curSecondShot].triCount; }
        }

        public int BufferSwapsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].bufferSwaps; }
        }

        public int ShaderSwapsPerSecond
        {
            get { return _secondSnaps[_curSecondShot].shaderSwaps; }
        }

        /// <summary>Gets the estimated amount of VRAM currently in use on the GPU.</summary>
        public long AllocatedVRAM { get { return _vramUsed; } }
    }
}
