using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpriteVertex : IVertexType
    {
        [VertexElement(VertexElementType.Vector2, VertexElementUsage.Position, 0)]
        public Vector2F Position;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.Position, 1)]
        public Vector2F Size;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.Position, 2)]
        public Vector2F Origin;

        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 3)] // UV start + end (two Vector2 values)
        public Vector4F UV;

        /// <summary>
        /// The rotation.
        /// </summary>
        [VertexElement(VertexElementType.Float, VertexElementUsage.Position, 4)]
        public float Rotation;

        /// <summary>
        /// The rotation (X) and array slice index (Y).
        /// </summary>
        [VertexElement(VertexElementType.Float, VertexElementUsage.Position, 5)]
        public float ArraySlice;

        [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
        public Color4 Color;
    }
}
