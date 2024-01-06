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
    public static GraphicsFormat ToGraphicsFormat(this VertexElementType type)
    {
        switch (type)
        {
            case VertexElementType.Float:
                return GraphicsFormat.R32_Float;

            case VertexElementType.Vector2:
                return GraphicsFormat.R32G32_Float;

            case VertexElementType.Vector3:
                return GraphicsFormat.R32G32B32_Float;

            case VertexElementType.Vector4:
                return GraphicsFormat.R32G32B32A32_Float;

            case VertexElementType.Color:
                return GraphicsFormat.R8G8B8A8_UNorm;

            case VertexElementType.Byte:
                return GraphicsFormat.R8_UInt;

            case VertexElementType.Byte4:
                return GraphicsFormat.R8G8B8A8_UInt;

            case VertexElementType.Short:
                return GraphicsFormat.R16_SInt;

            case VertexElementType.Short2:
                return GraphicsFormat.R16G16_SInt;

            case VertexElementType.Short4:
                return GraphicsFormat.R16G16B16A16_SInt;

            case VertexElementType.NormalizedShort2:
                return GraphicsFormat.R16G16_SNorm;

            case VertexElementType.NormalizedShort4:
                return GraphicsFormat.R16G16B16A16_SNorm;

            case VertexElementType.Half:
                return GraphicsFormat.R16_Float;

            case VertexElementType.Half2:
                return GraphicsFormat.R16G16_Float;

            case VertexElementType.Half4:
                return GraphicsFormat.R16G16B16A16_Float;

            case VertexElementType.Int:
                return GraphicsFormat.R32_SInt;

            case VertexElementType.Int2:
                return GraphicsFormat.R32G32_SInt;

            case VertexElementType.Int3:
                return GraphicsFormat.R32G32B32_SInt;

            case VertexElementType.Int4:
                return GraphicsFormat.R32G32B32A32_SInt;

            case VertexElementType.UInt:
                return GraphicsFormat.R32_UInt;

            case VertexElementType.UInt2:
                return GraphicsFormat.R32G32_UInt;

            case VertexElementType.UInt3:
                return GraphicsFormat.R32G32B32_UInt;

            case VertexElementType.UInt4:
                return GraphicsFormat.R32G32B32A32_UInt;

            default:
                throw new NotSupportedException("Unknown vertex element format!");
        }
    }
}
