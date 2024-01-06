namespace Molten.Graphics;

/// <summary>
/// Based on the D3D shader input type enum: D3D_SHADER_INPUT_TYPE.
/// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_shader_input_type</param>
/// </summary>
public enum ShaderInputType
{
    /// <summary>
    /// A constant buffer.
    /// </summary>
    CBuffer = 0,

    /// <summary>
    /// A texture buffer.
    /// </summary>
    TBuffer = 1,

    /// <summary>
    /// A texture.
    /// </summary>
    Texture = 2,

    /// <summary>
    /// A sampler.
    /// </summary>
    Sampler = 3,

    UavRWTyped = 4,

    Structured = 5,

    UavRWStructured = 6,

    ByteAddress = 7,

    UavRWByteAddress = 8,

    UavAppendStructured = 9,

    UavConsumeStructured = 10,

    UavRWStructuredWithCounter = 11,

    RTAccelerationStructure = 12,

    UavFeedbacktexture = 13,
}
