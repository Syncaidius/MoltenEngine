using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A placeholder vertex type for times when no vertex input is required.</summary>
    public struct VertexWithID : IVertexType
    {
        [VertexElement(VertexElementType.UInt, VertexElementUsage.VertexID, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        public uint Id;
    }
}
