namespace Molten.Graphics;

/// <summary>
/// Represents a type of shader stage.
/// </summary>
public enum ShaderType
{
    /// <summary>
    /// Unknown shader type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Vertex shader.
    /// </summary>
    Vertex = 1,

    /// <summary>
    /// Hull Shader. Required for tessellation.
    /// </summary>
    Hull = 2,

    /// <summary>
    /// Domain Shader. Required for tessellation.
    /// </summary>
    Domain = 3,

    /// <summary>
    /// Geometry shader.
    /// </summary>
    Geometry = 4,

    /// <summary>
    /// Pixel shader (fragment shader in OpenGL or Vulkan).
    /// </summary>
    Pixel = 5,

    /// <summary>
    /// Compute shader.
    /// </summary>
    Compute = 6,

    /// <summary>
    /// Amplification shader (Task shader in Vulkan). Used in conjunction with mesh shaders.
    /// </summary>
    Amplification = 7,

    /// <summary>
    /// Mesh shader. Used in conjunction with amplification/task shaders.
    /// </summary>
    Mesh = 8,

    /// <summary>
    /// Library shader. Required for DirectX Raytracing (DXR) shaders.
    /// See: https://docs.microsoft.com/en-us/windows/win32/direct3d12/direct3d-12-raytracing-hlsl-shaders
    /// </summary>
    Lib = 16,
}
