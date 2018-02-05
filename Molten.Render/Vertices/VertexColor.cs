using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A vertex type containing just position and color data.</summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct VertexColor : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        public Vector4 Position4;

        [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
        public Color Color;

        public VertexColor(Vector3 position, Color color)
        {
            this.Position4 = new Vector4(position, 1);
            this.Color = color;
        }

        public VertexColor(Vector4 position, Color color)
        {
            this.Position4 = position;
            this.Color = color;
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
            return (this == ((VertexColor)obj));
        }

        public static bool operator ==(VertexColor left, VertexColor right)
        {
            return ((left.Position4 == right.Position4) && (left.Color == right.Color));
        }

        public static bool operator !=(VertexColor left, VertexColor right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format("{Position:{0} Color:{1}}", new object[] { this.Position4, this.Color });
        }
    }
}
