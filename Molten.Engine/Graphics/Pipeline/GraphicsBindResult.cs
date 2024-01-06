namespace Molten.Graphics;

[Flags]
/// <summary>Represents possible validation results from the last graphics draw call.</summary>
public enum GraphicsBindResult : uint
{
    /// <summary>Nothing went wrong!</summary>
    Successful = 0,

    /// <summary>Vertex data was applied that does not match the input layout of the applied vertex shader, or vice versa.</summary>
    InvalidVertexLayout = 1,

    /// <summary>This only happens if you are trying to draw using indexed primitives. Indexed drawing requires a vertex buffer to refer to.</summary>
    MissingVertexSegment = 1 << 1,

    /// <summary>An attempt was made to draw indexed primitives without an index buffer applied.</summary>
    MissingIndexSegment = 1 << 2,

    /// <summary>An attempt was made to use a geometry shader that expects primitives with adjacency, while tessellation is active (hull shader applied).</summary>
    TessellationAdjacency = 1 << 3,

    /// <summary>A hull shader was applied without a domain shader. The reverse will not cause a failure because tessellation is only activated if a hull
    /// shader is applied (as per the DirectX SDK).</summary>
    MissingDomainShader = 1 << 4,

    /// <summary>A domain shader has been set, but the domain shader is missing.</summary>
    MissingHullShader = 1 << 5,

    /// <summary>An attempt was made to draw without a vertex shader applied, or it was invalid.</summary>
    MissingMaterial = 1 << 6,

    /// <summary>No pixel shader was applied or is invalid.</summary>
    MissingPixelShader = 1 << 7,

    /// <summary>The pixel effect/shader was present, but was invalid.</summary>
    InvalidPixelShader = 1 << 8,

    /// <summary>The shader was invalid or unsupported.</summary>
    InvalidShader = 1 << 9,

    /// <summary>The geometry shader was present, but invalid.</summary>
    InvalidGeometryShader = 1 << 10,

    /// <summary>The hull shader was present, but invalid.</summary>
    InvalidHullShader = 1 << 11,

    /// <summary>The domain shader was present, but invalid.</summary>
    InvalidDomainShader = 1 << 12,

    /// <summary>The input-output link between two or more shaders is invalid. Input and output must have the same layout.</summary>
    InvalidShaderIOChain = 1 << 13,

    /// <summary>
    /// No shader was set.
    /// </summary>
    NoShader = 1 << 14,

    /// <summary>The current setup of vertex buffers and vertex shader does not form a layout capable of handling per-instance data.</summary>
    NonInstancedVertexLayout = 1 << 15,

    /// <summary>The topology was undefined.</summary>
    UndefinedTopology = 1 << 16,

    /// <summary>
    /// A compute group dimension was invalid.
    /// </summary>
    InvalidComputeGroupDimension = 1 << 17,
}
