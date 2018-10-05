using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    /// <summary>A vertex type containing position, color, normal and UV data.</summary>
    public struct VertexColorNormalTexture : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        Vector4F Position4;

        [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
        public Color Color;

        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Normal, 0)]
        public Vector3F Normal;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.TextureCoordinate, 0)]
        public Vector2F UV;

        public VertexColorNormalTexture(Vector4F position, Color color, Vector3F normal, Vector2F textureCoordinates)
        {
            this.Position4 = position;
            this.Color = color;
            this.Normal = normal;
            this.UV = textureCoordinates;
        }

        public VertexColorNormalTexture(Vector3F position, Color color, Vector3F normal, Vector2F textureCoordinates)
        {
            this.Position4 = new Vector4F(position, 1);
            this.Color = color;
            this.Normal = normal;
            this.UV = textureCoordinates;
        }

        public VertexColorNormalTexture(Vector3F position, Vector3F normal, Vector2F textureCoordinates)
        {
            this.Position4 = new Vector4F(position, 1);
            this.Color = new Color(255,255,255,255);
            this.Normal = normal;
            this.UV = textureCoordinates;
        }

        public override string ToString()
        {
            return string.Format("{Position:{0} Color:{1} Normal: {2} UV: {3}}", new object[] { this.Position4, this.Color, this.Normal, this.UV });
        }
    }
}
