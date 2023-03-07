using System.Diagnostics;

namespace Molten.Graphics
{
    public class RenderProfiler
    {
        public class Snapshot
        {
            /// <summary>
            /// The number of draw calls made during a frame.
            /// </summary>
            public uint DrawCalls { get; set; }

            /// <summary>
            /// The number of compute dispatch calls made during a frame.
            /// </summary>
            public uint DispatchCalls { get; set; }


            /// <summary>
            /// The total number of pipeline slot bindings called during the frame. These are not GPU/API bindings.
            /// </summary>
            public uint SlotBindings { get; set; }

            /// <summary>
            /// The number of total GPU/API resource/object bindings called during a frame.
            /// </summary>
            public uint GpuBindings { get; set; }

            public uint BufferBindings { get; set; }

            public uint ShaderBindings { get; set; }

            public uint SurfaceBindings { get; set; }

            /// <summary>The total number of triangles that were rendered in the previous frame.</summary>
            public uint PrimitiveCount { get; set; }

            /// <summary>The time it took to render the previous frame.</summary>
            public double Time { get; set; }

            /// <summary>
            /// The target frame time during the frame that the snapshot was recorded.
            /// </summary>
            public double TargetTime { get; set; }

            /// <summary>
            /// Gets the frame's ID at the time the snapshot was saved.
            /// </summary>
            public ulong FrameID { get; set; }

            /// <summary>
            /// The amount of VRAM allocated during the frame.
            /// </summary>
            public ulong AllocatedVRAM { get; set; }

            public uint MapDiscardCount { get; set; }

            public uint MapNoOverwriteCount { get; set; }

            public uint MapWriteCount { get; set; }

            public uint MapReadCount { get; set; }

            public uint UpdateSubresourceCount { get; set; }

            public uint CopySubresourceCount { get; set; }

            public uint CopyResourceCount { get; set; }

            public void Clear()
            {
                DrawCalls = 0;
                DispatchCalls = 0;
                GpuBindings = 0;
                BufferBindings = 0;
                ShaderBindings = 0;
                SurfaceBindings = 0;
                PrimitiveCount = 0;
                Time = 0;
                TargetTime = 0;
                FrameID = 0;
                AllocatedVRAM = 0;
                MapDiscardCount = 0;
                MapNoOverwriteCount = 0;
                MapWriteCount = 0;
                MapReadCount = 0;
                UpdateSubresourceCount = 0;
                CopySubresourceCount = 0;
                CopyResourceCount = 0;
            }

            public void Accumulate(Snapshot other)
            {
                DrawCalls += other.DrawCalls;
                SlotBindings += other.SlotBindings;
                DispatchCalls += other.DispatchCalls;
                GpuBindings += other.GpuBindings;
                SurfaceBindings += other.SurfaceBindings;
                PrimitiveCount += other.PrimitiveCount;
                BufferBindings += other.BufferBindings;
                ShaderBindings += other.ShaderBindings;
                Time += other.Time;
                MapDiscardCount += other.MapDiscardCount;
                MapNoOverwriteCount += other.MapNoOverwriteCount;
                MapWriteCount += other.MapWriteCount;
                MapReadCount += other.MapReadCount;
                UpdateSubresourceCount += other.UpdateSubresourceCount;
                CopySubresourceCount += other.CopySubresourceCount;
                CopyResourceCount += other.CopyResourceCount;
            }
        }

        Snapshot[] _snapshots;
        Stopwatch _frameTimer;
        int _curID;

        public RenderProfiler(int maxSnapshots = 20)
        {
            if (maxSnapshots < 2)
                throw new Exception("The minimum number of snapshots is 2.");

            MaxSnapshots = maxSnapshots;
            _frameTimer = new Stopwatch();
            _snapshots = new Snapshot[maxSnapshots];
            for (int i = 0; i < maxSnapshots; i++)
                _snapshots[i] = new Snapshot();

            Current = _snapshots[_curID];
            Previous = _snapshots[_snapshots.Length - 1];
        }

        public void Clear()
        {
            _curID = 0;
            _frameTimer.Stop();
            _frameTimer.Reset();
            FrameID = 0;

            for (int i = 0; i < MaxSnapshots; i++)
                _snapshots[i].Clear();
        }

        public void Accumulate(Snapshot data)
        {
            Current.Accumulate(data);
        }

        public void Begin()
        {
            _frameTimer.Reset();
            _frameTimer.Start();
        }

        public void End(Timing time)
        {
            _frameTimer.Stop();
            Current.Time = _frameTimer.Elapsed.TotalMilliseconds;
            Current.TargetTime = time.TargetFrameTime;
            Current.FrameID = FrameID++;
            Previous = Current;
            _curID++;

            // Take the oldest snapshot and move it to the front for re-use.
            Current = _snapshots[0];
            if (_curID == MaxSnapshots)
            {
                _curID--;
                Array.Copy(_snapshots, 1, _snapshots, 0, _snapshots.Length - 1);
                _snapshots[_curID] = Current;
            }

            Current = _snapshots[_curID];
            Current.Clear();
        }

        /// <summary>
        /// The total number of frames tracked during this 
        /// </summary>
        public uint FrameID { get; private set; }

        /// <summary>
        /// Gets the profiling data for the current frame.
        /// </summary>
        public Snapshot Current { get; private set; }

        /// <summary>
        /// Gets the profiling data for the previous frame.
        /// </summary>
        public Snapshot Previous { get; private set; }

        /// <summary>
        /// Gets the maximum number of snapshots held by the <see cref="RenderProfiler"/>.
        /// </summary>
        public int MaxSnapshots { get; }
    }
}
