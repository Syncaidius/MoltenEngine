using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpriteVertex : IVertexType
    {
        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Position, 0)]
        public Vector3F Position;

        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Position, 1)]
        public Vector3F Rotation;

        [VertexElement(VertexElementType.Vector2, VertexElementUsage.Position, 2)]
        public Vector2F Size;

        /// <summary>
        /// The origin of the sprite. The X and Y axis values contain the origin, while the Z-axis value contains the UV texture array slice index.
        /// </summary>
        [VertexElement(VertexElementType.Vector3, VertexElementUsage.Position, 3)]
        public Vector3F Origin;

        [VertexElement(VertexElementType.Vector4, VertexElementUsage.Position, 4)] // UV start + end (two Vector2 values)
        public Vector4F UV;

        [VertexElement(VertexElementType.Color, VertexElementUsage.Color, 0)]
        public Color Color;
    }
}
