using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public sealed class VertexFormat
    {
        internal InputElement[] Elements;

        public VertexFormat(InputElement[] elements, int sizeOf, int hash)
        {
            SizeOf = sizeOf;
            Elements = elements;
            HashKey = hash;
        }

        /// <summary>Gets the total size of the Vertex Format, in bytes.</summary>
        public int SizeOf { get; private set; }

        /// <summary>Gets the hash key associated with the vertex format instance.</summary>
        public int HashKey { get; private set; }
    }
}
