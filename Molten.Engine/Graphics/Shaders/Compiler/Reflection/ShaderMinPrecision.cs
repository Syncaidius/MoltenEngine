namespace Molten.Graphics;

/// <summary>
/// Based on the D3D shader register component type enum: D3D_MIN_PRECISION.
/// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_min_precision</param>
/// </summary>
public enum ShaderMinPrecision
{
    Default = 0,

    Float16 = 1,

    Float28 = 2,

    Reserved = 3,

    Sint16 = 4,

    Uint16 = 5,

    Any16 = 240,

    Any10 = 241
}
