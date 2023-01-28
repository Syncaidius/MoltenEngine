namespace Molten.Graphics
{
    /// <summary>
    /// Based on the D3D shader variable type enum: D3D_SHADER_VARIABLE_TYPE .
    /// <param>See: https://learn.microsoft.com/en-us/windows/win32/api/d3dcommon/ne-d3dcommon-d3d_shader_variable_type</param>
    /// </summary>
    public enum ShaderVariableClass
    {
        Scalar = 0,

        Vector = 1,

        MatrixRows = 2,

        MatrixColumns = 3,

        Object = 4,

        Struct = 5,

        InterfaceClass = 6,

        InterfacePointer = 7,

        ForceDword = int.MaxValue
    }
}
