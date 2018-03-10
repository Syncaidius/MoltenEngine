using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    /// <summary>A vertex type containing just position and color data.</summary>
    public struct VertexTexture : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        public Vector4F Position4;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.TextureCoordinate, 0)]
        public Vector2F UV;

        public VertexTexture(Vector4F position, Vector2F textureCoordinates)
        {
            this.Position4 = position;
            this.UV = textureCoordinates;
        }

        public VertexTexture(Vector3F position, Vector2F textureCoordinates)
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
            return (this == ((VertexTexture)obj));
        }

        public static bool operator ==(VertexTexture left, VertexTexture right)
        {
            return ((left.Position4 == right.Position4) && (left.UV == right.UV));
        }

        public static bool operator !=(VertexTexture left, VertexTexture right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format("{Position:{0} Color:{1}}", new object[] { this.Position4, this.UV });
        }
    }
}
