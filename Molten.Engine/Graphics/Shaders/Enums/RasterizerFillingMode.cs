namespace Molten;

/// <summary>
/// Represents available rasterizer filling modes.
/// </summary>
public enum RasterizerFillingMode
{
    /// <summary>
    /// Does not rasterizer primitives.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Only fills the edges of primitives.
    /// </summary>
    Wireframe = 0x2,

    /// <summary>
    /// Fills primitives as usual.
    /// </summary>
    Solid = 0x3,

    /// <summary>
    /// Only fills an area around each vertex that makes up the primitive.
    /// </summary>
    Point = 0x16,
}
