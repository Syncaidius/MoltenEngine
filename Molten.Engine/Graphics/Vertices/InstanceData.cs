using Molten.IO;
using System.Runtime.InteropServices;

namespace Molten.Graphics;

/// <summary>A vertex type containing just position and color data.</summary>
[StructLayout(LayoutKind.Sequential, Pack =1)]
public struct InstanceData : IVertexType, IVertexInstanceType
{
    /// <summary>The first row of a world, view, projection matrix</summary>
    [VertexElement(VertexElementType.Vector4, VertexElementUsage.Custom, 0, VertexInputType.PerInstanceData, 1, "WORLD")]
    public Vector4F WvpX;

    /// <summary>The second row of a world, view, projection matrix</summary>
    [VertexElement(VertexElementType.Vector4, VertexElementUsage.Custom, 1, VertexInputType.PerInstanceData, 1, "WORLD")]
    public Vector4F WvpY;

    /// <summary>The third row of a world, view, projection matrix</summary>
    [VertexElement(VertexElementType.Vector4, VertexElementUsage.Custom, 2, VertexInputType.PerInstanceData, 1, "WORLD")]
    public Vector4F WvpZ;

    /// <summary>The forth row of a world, view, projection matrix</summary>
    [VertexElement(VertexElementType.Vector4, VertexElementUsage.Custom, 3, VertexInputType.PerInstanceData, 1, "WORLD")]
    public Vector4F WvpW;

    public InstanceData(Matrix4F world)
    {
        WvpX = world.Row1;
        WvpY = world.Row2;
        WvpZ = world.Row3;
        WvpW = world.Row4;
    }

    public static void WriteBatchData(RawStream stream, ObjectRenderData data)
    {
        stream.Write(ref data.RenderTransform);
    }

    /// <inheritdoc/>
    public static bool IsBatched { get; } = true;
}
