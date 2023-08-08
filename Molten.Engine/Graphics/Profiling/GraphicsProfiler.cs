using Molten.Font;

namespace Molten.Graphics
{
    public class GraphicsProfiler
    {
        public class FrameProfile : GraphicsDeviceProfiler
        {
            public TimeSpan TimeTaken { get; internal set; }

            public TimeSpan TargetTime { get; internal set; }

            public ulong FrameID { get; internal set; }
        }

        FrameProfile[] _frames;
        uint _index;

        internal GraphicsProfiler(Timing time, uint historyCount)
        {
            _frames = new FrameProfile[historyCount];
            for(int i = 0; i < _frames.Length; i++)
                _frames[i] = new FrameProfile();

            // Setup the first frame. Since we have no previous frame yet, we'll use the first again.
            Previous = _frames[_index];
            Previous.TimeTaken = time.TotalTime;
            Previous.TargetTime = time.TargetFrameTime;
            Previous.FrameID = time.FrameID;
        }

        internal void Accumulate(GraphicsQueueProfiler other)
        {
            FrameProfile p = _frames[_index];
            p.Accumulate(other);

            other.Reset();
        }

        internal void NextFrame(Timing time, ulong frameID)
        {
            Previous = _frames[_index];
            _frames[_index].TimeTaken = time.TotalTime;
            _frames[_index++].FrameID = frameID;

            if (_index >= _frames.Length)
                _index = 0;
        }

        /// <summary>
        /// Gets profiling data for the previous frame.
        /// <para>It is not possible to retrieve the current frame's profiling data, as it will not be complete.</para>
        /// </summary>
        public FrameProfile Previous { get; private set; }
    }
}
