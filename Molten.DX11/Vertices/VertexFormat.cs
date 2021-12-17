using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public sealed class VertexFormat : EngineObject
    {
        internal InputElementDesc[] Elements;

        public VertexFormat(InputElementDesc[] elements, uint sizeOf)
        {
            SizeOf = sizeOf;
            Elements = elements;
        }

        protected unsafe override void OnDispose()
        {
            // Dispose of element string pointers, since they were statically-allocated by Silk.NET
            for (uint i = 0; i < Elements.Length; i++)
                SilkMarshal.Free((nint)Elements[i].SemanticName);
        }

        /// <summary>Gets the total size of the Vertex Format, in bytes.</summary>
        public uint SizeOf { get; private set; }
    }
}
