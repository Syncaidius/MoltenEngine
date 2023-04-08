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

        public GraphicsQueue Queue { get; }

        public GraphicsFence Fence { get; set; }
    }
}
