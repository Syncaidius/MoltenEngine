using System.Runtime.InteropServices;
using Molten.Graphics;

namespace Molten.Examples
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    /// <summary>A vertex type containing just position and color data.</summary>
    public struct CubeArrayVertex : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        public Vector4F Position4;

        [VertexElement(VertexElementType.Vector3, VertexElementUsage.TextureCoordinate, 0)]
        public Vector3F UV;

        public CubeArrayVertex(Vector4F position, Vector3F textureCoordinates)
        {
            this.Position4 = position;
            this.UV = textureCoordinates;
        }

        public CubeArrayVertex(Vector3F position, Vector3F textureCoordinates)
        {
            this.Position4 = new Vector4F(position, 1);
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
            return (this == ((CubeArrayVertex)obj));
        }

        public static bool operator ==(CubeArrayVertex left, CubeArrayVertex right)
        {
            return ((left.Position4 == right.Position4) && (left.UV == right.UV));
        }

        public static bool operator !=(CubeArrayVertex left, CubeArrayVertex right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format("{Position:{0} Color:{1}}", new object[] { this.Position4, this.UV });
        }
    }
}

