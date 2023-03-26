using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum GraphicsBufferType
    {
        Unknown = 0,

        VertexBuffer = 1,

        IndexBuffer = 2,

        ByteAddressBuffer = 3,

        StructuredBuffer = 4,

        StagingBuffer = 5,

        /// <summary>
        /// Constant or uniform buffer
        /// </summary>
        ConstantBuffer = 6,
    }
}
