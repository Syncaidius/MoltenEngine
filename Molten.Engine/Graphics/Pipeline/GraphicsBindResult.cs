namespace Molten.Graphics
{
    [Flags]
    /// <summary>Represents possible validation results from the last graphics draw call.</summary>
    public enum GraphicsBindResult : uint
    {
        /// <summary>Nothing went wrong!</summary>
        Successful = 0,

        /// <summary>Vertex data was applied that does not match the input layout of the applied vertex shader, or vice versa.</summary>
        InvalidVertexLayout = 1,

        /// <summary>This only happens if you are trying to draw using indexed primitives. Indexed drawing requires a vertex buffer to refer to.</summary>
        MissingVertexSegment = 2,

        /// <summary>An attempt was made to draw indexed primitives without an index buffer applied.</summary>
        MissingIndexSegment = 4,

        /// <summary>An attempt was made to use a geometry shader that expects primitives with adjacency, while tessellation is active (hull shader applied).</summary>
        TessellationAdjacency = 8,

        /// <summary>A hull shader was applied without a domain shader. The reverse will not cause a failure because tessellation is only activated if a hull
        /// shader is applied (as per the DirectX SDK).</summary>
        MissingDomainShader = 16,

        /// <summary>A domain shader has been set, but the domain shader is missing.</summary>
        MissingHullShader = 32,

        /// <summary>An attempt was made to draw without a vertex shader applied, or it was invalid.</summary>
        MissingMaterial = 64,

        /// <summary>No pixel shader was applied or is invalid.</summary>
        MissingPixelShader = 128,

        /// <summary>The pixel effect/shader was present, but was invalid.</summary>
        InvalidPixelShader = 256,

        /// <summary>The vertex shader was present, but invalid.</summary>
        InvalidMaterial = 512,

        /// <summary>The geometry shader was present, but invalid.</summary>
        InvalidGeometryShader = 1024,

        /// <summary>The hull shader was present, but invalid.</summary>
        InvalidHullShader = 2048,

        /// <summary>The domain shader was present, but invalid.</summary>
        InvalidDomainShader = 4096,

        /// <summary>A draw attempt was made while tessellation was active (hull shader applied), but the provided 
        /// vertex buffer was not of a patch topology.</summary>
        HullPatchTopologyExpected = 8192,

        /// <summary>The input-output link between two or more shaders is invalid. Input and output must have the same layout.</summary>
        InvalidShaderIOChain = 16384,

        /// <summary>The current setup of vertex buffers and vertex shader does not form a layout capable of handling per-instance data.</summary>
        NonInstancedVertexLayout = 32768,

        /// <summary>The topology was undefined.</summary>
        UndefinedTopology = 65536,
    }
}
