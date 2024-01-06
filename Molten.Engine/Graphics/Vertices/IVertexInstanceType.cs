using Molten.IO;

namespace Molten.Graphics;

/// <summary>Defines a vertex type that can be used with Vertex buffers.</summary>
public interface IVertexInstanceType : IVertexType
{
    static abstract void WriteBatchData(RawStream stream, ObjectRenderData data);

    /// <summary>
    /// Gets whether or not the vertex instance type should be automatically batch.
    /// </summary>
    static abstract bool IsBatched { get; }
}
