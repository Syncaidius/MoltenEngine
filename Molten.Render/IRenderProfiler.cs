using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IRenderProfiler
    {
        /// <summary>Gets stats for the previous frame.</summary>
        RenderFrameSnapshot PreviousFrame { get; }

        /// <summary>Gets stats for the current frame.</summary>
        RenderFrameSnapshot CurrentFrame { get; }

        /// <summary>Gets stats accumulated across all frames within the previous second of time.</summary>
        RenderFrameSnapshot PreviousSecond { get; }

        /// <summary>Gets the stats so far for the current second of time being measured.</summary>
        RenderFrameSnapshot CurrentSecond { get; }

        /// <summary>Gets the estimated amount of VRAM currently in use on the GPU.</summary>
        long AllocatedVRAM { get; }
    }
}
