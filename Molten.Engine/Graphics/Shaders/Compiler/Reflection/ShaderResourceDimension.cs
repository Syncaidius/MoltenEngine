namespace Molten.Graphics;

/// <summary>
/// Based on the D3D shader input type enum: D3D_SRV_DIMENSION.
/// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_srv_dimension</param>
/// </summary>
public enum ShaderResourceDimension
{
    Unknown = 0,

    Buffer = 1,

    Texture1D = 2,

    Texture1DArray = 3,

    Texture2D = 4,

    Texture2DArray = 5,

    Texture2DMS = 6,

    Texture2DMSArray = 7,

    Texture3D = 8,

    TextureCube = 9,

    TextureCubeArray = 10,

    BufferEx = 11,
}
