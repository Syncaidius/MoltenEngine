using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public unsafe abstract class GraphicsResourceHandle : IDisposable
    {
        public abstract void Dispose();

        public abstract void* Ptr { get; }
    }
}
