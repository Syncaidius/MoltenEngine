namespace Molten.Graphics;

public enum VertexElementType
{
    Float,

    Vector2,

    Vector3,

    Vector4,

    Color,

    Byte,

    Byte4,

    Short,

    Short2,

    Short4,

    NormalizedShort2,

    NormalizedShort4,

    Half,

    Half2,

    Half4,

    Int,

    Int2,

    Int3,

    Int4,

    UInt,

    UInt2,

    UInt3,

    UInt4,
}

public static class VertexElementTypeExtensions
{
    public static GraphicsFormat ToGraphicsFormat(this VertexElementType type) => type switch
    {
        VertexElementType.Float => GraphicsFormat.R32_Float,
        VertexElementType.Vector2 => GraphicsFormat.R32G32_Float,
        VertexElementType.Vector3 => GraphicsFormat.R32G32B32_Float,
        VertexElementType.Vector4 => GraphicsFormat.R32G32B32A32_Float,
        VertexElementType.Color => GraphicsFormat.R8G8B8A8_UNorm,
        VertexElementType.Byte => GraphicsFormat.R8_UInt,
        VertexElementType.Byte4 => GraphicsFormat.R8G8B8A8_UInt,
        VertexElementType.Short => GraphicsFormat.R16_SInt,
        VertexElementType.Short2 => GraphicsFormat.R16G16_SInt,
        VertexElementType.Short4 => GraphicsFormat.R16G16B16A16_SInt,
        VertexElementType.NormalizedShort2 => GraphicsFormat.R16G16_SNorm,
        VertexElementType.NormalizedShort4 => GraphicsFormat.R16G16B16A16_SNorm,
        VertexElementType.Half => GraphicsFormat.R16_Float,
        VertexElementType.Half2 => GraphicsFormat.R16G16_Float,
        VertexElementType.Half4 => GraphicsFormat.R16G16B16A16_Float,
        VertexElementType.Int => GraphicsFormat.R32_SInt,
        VertexElementType.Int2 => GraphicsFormat.R32G32_SInt,
        VertexElementType.Int3 => GraphicsFormat.R32G32B32_SInt,
        VertexElementType.Int4 => GraphicsFormat.R32G32B32A32_SInt,
        VertexElementType.UInt => GraphicsFormat.R32_UInt,
        VertexElementType.UInt2 => GraphicsFormat.R32G32_UInt,
        VertexElementType.UInt3 => GraphicsFormat.R32G32B32_UInt,
        VertexElementType.UInt4 => GraphicsFormat.R32G32B32A32_UInt,
        _ => throw new NotSupportedException("Unknown vertex element format!"),
    };

    /// <summary>
    /// Gets the number of components in the given vertex element type.
    /// </summary>
    /// <param name="type">The element type.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static uint GetComponentCount(this VertexElementType type) => type switch
    {
        VertexElementType.Int => 1,
        VertexElementType.UInt => 1,
        VertexElementType.Half => 1,
        VertexElementType.Byte => 1,
        VertexElementType.Short => 1,
        VertexElementType.Float => 1,
        VertexElementType.Vector2 => 2,
        VertexElementType.Vector3 => 3,
        VertexElementType.Vector4 => 4,

        VertexElementType.Color => 4,
        VertexElementType.Byte4 => 4,

        VertexElementType.Short2 => 2,
        VertexElementType.Short4 => 4,

        VertexElementType.Int2 => 2,
        VertexElementType.Int3 => 3,
        VertexElementType.Int4 => 4,

        VertexElementType.UInt2 => 2,
        VertexElementType.UInt3 => 3,
        VertexElementType.UInt4 => 4,
        _ => throw new NotSupportedException($"Unsupported vertex element type: {type}"),
    };
}
