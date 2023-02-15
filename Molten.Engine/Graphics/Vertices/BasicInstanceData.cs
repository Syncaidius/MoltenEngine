using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    /// <summary>A vertex type containing just position and color data.</summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct BasicInstanceData : IVertexType
    {
        /// <summary>The first row of a world, view, projection matrix</summary>
        [VertexElement(VertexElementType.Float, VertexElementUsage.Custom, 0, VertexInputType.PerInstanceData, "ROWX")]
        [FieldOffset(0)]
        public Vector4F WvpX;

        /// <summary>The second row of a world, view, projection matrix</summary>
        [VertexElement(VertexElementType.Float, VertexElementUsage.Custom, 0, VertexInputType.PerInstanceData, "ROWY")]
        [FieldOffset(0)]
        public Vector4F WvpY;

        /// <summary>The third row of a world, view, projection matrix</summary>
        [VertexElement(VertexElementType.Float, VertexElementUsage.Custom, 0, VertexInputType.PerInstanceData, "ROWZ")]
        [FieldOffset(0)]
        public Vector4F WvpZ;

        /// <summary>The forth row of a world, view, projection matrix</summary>
        [VertexElement(VertexElementType.Float, VertexElementUsage.Custom, 0, VertexInputType.PerInstanceData, "ROWW")]
        [FieldOffset(0)]
        public Vector4F WvpW;

        public BasicInstanceData(Matrix4F world)
        {
            WvpX = world.Row1;
            WvpY = world.Row2;
            WvpZ = world.Row3;
            WvpW = world.Row4;
        }
    }
}
