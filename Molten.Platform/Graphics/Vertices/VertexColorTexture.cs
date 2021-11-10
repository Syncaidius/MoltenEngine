using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>A vertex type containing just position and color data.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexColorTexture : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        Vector4F Position4;

        [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
        public Color Color;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.TextureCoordinate, 0)]
        public Vector2F UV;

        public VertexColorTexture(Vector4F position, Color color, Vector2F textureCoordinates)
        {
            this.Position4 = position;
            this.Color = color;
            this.UV = textureCoordinates;
        }

        public VertexColorTexture(Vector3F position, Color color, Vector2F textureCoordinates)
        {
            this.Position4 = new Vector4F(position, 1);
            this.Color = color;
            this.UV = textureCoordinates;
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
            return (this == ((VertexColorTexture)obj));
        }

        public static bool operator ==(VertexColorTexture left, VertexColorTexture right)
        {
            return ((left.Position4 == right.Position4) && (left.Color == right.Color) && (left.UV == right.UV));
        }

        public static bool operator !=(VertexColorTexture left, VertexColorTexture right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format("{Position:{0} Color:{1}}", new object[] { this.Position4, this.UV });
        }
    }
}
