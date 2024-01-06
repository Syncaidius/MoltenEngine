namespace Molten.Graphics;

/// <summary>
/// Represents the capabilities and limits of a type of buffer resource.
/// </summary>
public class VertexBufferCapabilities : BufferCapabilities
{
    internal VertexBufferCapabilities() { }

    /// <summary>
    /// Gets or sets the maximum number of elements per vertex in the buffer.
    /// </summary>
    public uint MaxElementsPerVertex { get; set; }

    /// <summary>
    /// Gets or sets the maximum byte size of a single vertex in the buffer.
    /// </summary>
    public uint MaxElementStride { get; set; }
}
