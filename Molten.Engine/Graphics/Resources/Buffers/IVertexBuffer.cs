using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IVertexBuffer : IGraphicsBuffer
    {
        /// <summary>
        /// Gets the vertex format of the current <see cref="IVertexBuffer"/>, if any. This is set during creation.
        /// </summary>
        VertexFormat VertexFormat { get; }
    }
}
