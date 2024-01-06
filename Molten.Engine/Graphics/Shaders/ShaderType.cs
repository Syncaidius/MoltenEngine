namespace Molten.Graphics;

public enum ShaderType
{
    /// <summary>
    /// Unknown shader type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Pixel shader.
    /// </summary>
    Pixel = 1,

    /// <summary>
    /// Vertex shader.
    /// </summary>
    Vertex = 2,

    /// <summary>
    /// Geometry shader.
    /// </summary>
    Geometry = 3,

    /// <summary>
    /// Domain Shader. Required for tessellation.
    /// </summary>
    Domain = 4,

    /// <summary>
    /// Compute shader.
    /// </summary>
    Compute = 5,

    /// <summary>
    /// Hull Shader. Required for tessellation.
    /// </summary>
    Hull = 6,

    /// <summary>
    /// Library shader. Required for DirectX Raytracing (DXR) shaders.
    /// See: https://docs.microsoft.com/en-us/windows/win32/direct3d12/direct3d-12-raytracing-hlsl-shaders
    /// </summary>
    Lib = 7,
}
