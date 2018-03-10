using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    /// <summary>A vertex type containing position, color, normal and UV data.</summary>
    public struct EngineVertex : IVertexType
    {
        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 0)]
        /// <summary>Gets or sets the position as a Vector4</summary>
        Vector4F Position4;

        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Normal, 0)]
        public Vector3F Normal;

        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Tangent, 0)]
        public Vector3F Tangent;

        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Binormal, 0)]
        public Vector3F BiNormal;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.TextureCoordinate, 0)]
        public Vector2F UV;

        public EngineVertex(Vector4F position, Vector3F normal, Vector2F textureCoordinates)
        {
            this.Position4 = position;
            this.Normal = normal;
            this.Tangent = new Vector3F();
            this.BiNormal = new Vector3F();
            this.UV = textureCoordinates;
        }

        public EngineVertex(Vector3F position, Vector3F normal, Vector2F textureCoordinates)
        {
            this.Position4 = new Vector4F(position, 1);
            this.Normal = normal;
            this.Tangent = new Vector3F();
            this.BiNormal = new Vector3F();
            this.UV = textureCoordinates;
        }

        public override string ToString()
        {
            return string.Format("[Position:{0} Normal: {1} Tan: {2} BiN: {3} UV: {4}]", new object[] { this.Position4, 
                this.Normal, this.Tangent, 
                this.BiNormal, this.UV });
        }
    }
}
