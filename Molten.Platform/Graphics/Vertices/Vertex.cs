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
        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        public Vector3F Position;


        public Vertex(float x, float y, float z)
        {
            Position = new Vector3F(x,y,z);
        }

        public Vertex(Vector3F position)
        {
            Position = position;
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
            return ((left.Position == right.Position));
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Vertex -- {Position.X}, {Position.Y}, {Position.Z}";
        }
    }
}
