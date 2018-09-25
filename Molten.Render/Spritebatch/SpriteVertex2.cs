using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct SpriteVertex2 : IVertexType
    {
        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Position, 0)]
        public Vector3F Position;

        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Position, 4)]
        public Vector3F Rotation;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.Position, 1)]
        public Vector2F Size;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.Position, 2)]
        public Vector2F Origin;

        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 3)] // UV start + end (two Vector2 values)
        public Vector4F UV;

        [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
        public Color Color;
    }
}
