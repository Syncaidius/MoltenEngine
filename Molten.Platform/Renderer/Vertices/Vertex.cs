using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    /// <summary>A vertex type containing just position and color data.</summary>
    public struct Vertex : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        Vector4F Position4;

        public Vertex(Vector4F position, Vector2F textureCoordinates)
        {
            this.Position4 = position;
        }

        public Vertex(Vector3F position, Vector2F textureCoordinates)
        {
            this.Position4 = new Vector4F(position, 1);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((Vertex)obj));
        }

        public static bool operator ==(Vertex left, Vertex right)
        {
            return ((left.Position4 == right.Position4));
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format("{0}", new object[] { this.Position4 });
        }
    }
}
