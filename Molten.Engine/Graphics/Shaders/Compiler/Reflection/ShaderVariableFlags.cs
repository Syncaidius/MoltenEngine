namespace Molten.Graphics
{
    /// <summary>
    /// Based on the D3D shader variable flags enum: D3D_SHADER_VARIABLE_FLAGS .
    /// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_shader_variable_flags</param>
    /// </summary>
    public enum ShaderVariableFlags
    {
        None = 0,
        UserPacked = 1,
        Used = 2,
        InterfacePointer = 4,
        InterfaceParameter = 8,
        ForceDWord = 0x7fffffff
    }
}
