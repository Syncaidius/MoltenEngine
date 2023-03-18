using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ITypedBuffer : IStructuredBuffer
    {
        /// <summary>
        /// Gets the format of the current <see cref="ITypedBuffer"/>.
        /// </summary>
        TypedBufferFormat TypedFormat { get; }
    }
}
