using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct RenderFrameSnapshot
    {
        public int DrawCalls;
        public int Bindings;
        public int BufferSwaps;
        public int ShaderSwaps;
        public int RTSwaps;

        /// <summary>The total number of triangles that were rendered in the previous frame.</summary>
        public int TriCount;

        /// <summary>The time it took to render the previous frame.</summary>
        public double Time;

        /// <summary>
        /// The target frame time during the frame that the snapshot was recorded.
        /// </summary>
        public double TargetTime;

        /// <summary>
        /// Gets the frame's ID at the time the snapshot was saved.
        /// </summary>
        public ulong FrameID;

        /// <summary>
        /// The amount of VRAM allocated during the frame.
        /// </summary>
        public ulong AllocatedVRAM;

        public int MapDiscardCount;

        public int MapNoOverwriteCount;

        public int MapWriteCount;

        public int MapReadCount;

        public int UpdateSubresourceCount;

        public int CopySubresourceCount;

        public void Add(RenderFrameSnapshot other)
        {
            DrawCalls += other.DrawCalls;
            Bindings += other.Bindings;
            RTSwaps += other.RTSwaps;
            TriCount += other.TriCount;
            BufferSwaps += other.BufferSwaps;
            ShaderSwaps += other.ShaderSwaps;
            Time += other.Time;
            MapDiscardCount += other.MapDiscardCount;
            MapNoOverwriteCount += other.MapNoOverwriteCount;
            MapWriteCount += other.MapWriteCount;
            MapReadCount += other.MapReadCount;
            UpdateSubresourceCount += other.UpdateSubresourceCount;
            CopySubresourceCount += other.CopySubresourceCount;
        }
    }
}
