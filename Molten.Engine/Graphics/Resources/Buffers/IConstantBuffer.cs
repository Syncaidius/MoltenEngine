namespace Molten.Graphics;

public interface IConstantBuffer : IGraphicsResource
{
    /// <summary>
    /// Gets the name of the constant buffer.
    /// </summary>
    string BufferName { get; }

    /// <summary>
    /// Gets or sets whether or not the constant buffer is dirty.
    /// </summary>
    bool IsDirty { get; set; }

    /// <summary>
    /// Gets a list of variables that represent elements within the constant buffer.
    /// </summary>
    GraphicsConstantVariable[] Variables { get; }
}
