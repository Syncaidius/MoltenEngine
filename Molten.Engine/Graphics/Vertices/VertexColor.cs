using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>A vertex type containing just position and color data.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexColor : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        public Vector4F Position4;

        [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
        public Color Color;

        public VertexColor(Vector3F position, Color color)
        {
            Position4 = new Vector4F(position, 1);
            Color = color;
        }

        public VertexColor(Vector4F position, Color color)
        {
            Position4 = position;
            Color = color;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            return (this == (VertexColor)obj);
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
