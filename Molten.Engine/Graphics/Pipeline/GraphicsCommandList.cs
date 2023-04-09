using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsCommandList : EngineObject
    {
        protected GraphicsCommandList(GraphicsQueue queue)
        {
            
            Queue = queue;
        }

        public abstract void Free();

        public GraphicsQueue Queue { get; }

        public GraphicsFence Fence { get; set; }

        public uint BranchIndex { get; set; }

        public GraphicsCommandListFlags Flags { get; set; }

        public GraphicsCommandList Previous { get; internal set; }
    }
}
