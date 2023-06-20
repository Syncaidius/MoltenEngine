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

        protected GraphicsResourceHandle(GraphicsResource resource)
        {
            Resource = resource;
        }

        /// <summary>
        /// Gets the <see cref="GraphicsResource"/> that this handle is associated with.
        /// </summary>
        public GraphicsResource Resource { get; }

        /// <summary>
        /// Gets a pointer to the underlying native resource.
        /// </summary>
        public abstract void* Ptr { get; }
    }
}
