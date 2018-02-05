using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct RenderFrameSnapshot
    {
        public int drawCalls;
        public int bindings;
        public int bufferSwaps;
        public int shaderSwaps;
        public int rtSwaps;
        public int triCount;

        public void Add(RenderFrameSnapshot other)
        {
            drawCalls += other.drawCalls;
            bindings += other.bindings;
            rtSwaps += other.rtSwaps;
            triCount += other.triCount;
            bufferSwaps += other.bufferSwaps;
            shaderSwaps += other.shaderSwaps;
        }
    }
}
