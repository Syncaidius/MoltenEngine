namespace Molten.Graphics
{
    public enum ShaderType
    {
        /// <summary>
        /// Unknown shader type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Pixel shader.
        /// </summary>
        PixelShader = 1,

        /// <summary>
        /// Vertex shader.
        /// </summary>
        VertexShader = 2,

        /// <summary>
        /// Geometry shader.
        /// </summary>
        GeometryShader = 3,

        /// <summary>
        /// Domain Shader. Required for tessellation.
        /// </summary>
        DomainShader = 4,

        /// <summary>
        /// Compute shader.
        /// </summary>
        ComputeShader = 5,

        /// <summary>
        /// Hull Shader. Required for tessellation.
        /// </summary>
        HullShader = 6,

        /// <summary>
        /// Library shader. Required for DirectX Raytracing (DXR) shaders.
        /// See: https://docs.microsoft.com/en-us/windows/win32/direct3d12/direct3d-12-raytracing-hlsl-shaders
        /// </summary>
        LibShader = 7,
    }
}
