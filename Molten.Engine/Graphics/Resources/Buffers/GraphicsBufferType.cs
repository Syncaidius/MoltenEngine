namespace Molten.Graphics;

public enum GraphicsBufferType
{
    /// <summary>
    /// The buffer stores unknown data. This may be used for buffers that will be sub-allocated into smaller buffer regions.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The buffer is used to store vertex data.
    /// </summary>
    Vertex = 1,

    /// <summary>
    /// The buffer is used to store index data.
    /// </summary>
    Index = 2,

    ByteAddress = 3,

    /// <summary>
    /// The buffer is used to store structured data of a single type.
    /// </summary>
    Structured = 4,

    /// <summary>
    /// The buffer is used to store staging data.
    /// </summary>
    Staging = 5,

    /// <summary>
    /// The buffer is used to store constant or uniform data.
    /// </summary>
    Constant = 6,
}
