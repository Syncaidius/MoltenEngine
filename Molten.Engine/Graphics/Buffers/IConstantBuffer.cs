using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IConstantBuffer : IGraphicsBuffer
    {
        string BufferName { get; }
    }
}
