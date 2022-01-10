using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    /// <summary>A vertex type containing position, color, normal and UV data.</summary>
    public struct EngineBonedVertex : IVertexType
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

        [VertexElement(VertexElementType.Vector4, VertexElementUsage.BlendWeight, 0)]
        public Vector4F BlendWeights;

        [VertexElement(VertexElementType.Int4, VertexElementUsage.BlendIndices, 0)]
        public Vector4I BlendIndices;

        [VertexElement(VertexElementType.Int, VertexElementUsage.BlendIndices, 1)]
        public int BlendCount;

        public EngineBonedVertex(Vector4F position, Vector3F normal, Vector2F textureCoordinates)
        {
            this.Position4 = position;
            this.Normal = normal;
            this.Tangent = new Vector3F();
            this.BiNormal = new Vector3F();
            this.UV = textureCoordinates;
            this.BlendWeights = new Vector4F();
            this.BlendIndices = new Vector4I();
            this.BlendCount = 0;
        }

        public EngineBonedVertex(Vector3F position, Vector3F normal, Vector2F textureCoordinates)
        {
            this.Position4 = new Vector4F(position, 1);
            this.Normal = normal;
            this.Tangent = new Vector3F();
            this.BiNormal = new Vector3F();
            this.UV = textureCoordinates;
            this.BlendWeights = new Vector4F();
            this.BlendIndices = new Vector4I();
            this.BlendCount = 0;
        }

        public override string ToString()
        {
            return $"{{Position:{Position4} Normal: {Normal} Tan: {Tangent} BiN: {BiNormal} UV: {UV}}}";
        }
    }
}
