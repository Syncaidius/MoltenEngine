namespace Molten.Graphics;

/// <summary>
/// Based on the D3D shader register component type enum: D3D_REGISTER_COMPONENT_TYPE.
/// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_register_component_type</param>
/// </summary>
public enum ShaderRegisterType
{
    None = 0x0,

    Unknown = 0x0,

    UInt32 = 0x1,

    SInt32 = 0x2,

    Float32 = 0x3,

    UInt16 = 0x4,

    SInt16 = 0x5,

    Float16 = 0x6,

    UInt8 = 0x7,

    SInt8 = 0x8,

    UInt64 = 0x9,

    SInt64 = 0xA,

    Float64 = 0xB,
}
