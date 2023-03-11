using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IIndexBuffer : IGraphicsBuffer
    {
        /// <summary>
        /// Gets the <see cref="IndexBufferFormat"/> of the current <see cref="IIndexBuffer"/>. This is set during creation.
        /// </summary>
        IndexBufferFormat IndexFormat { get; }
    }
}
