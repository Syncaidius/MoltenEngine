namespace Molten.Graphics;

/// <summary>
/// Information about a shader's input or output parameters.
/// </summary>
public class ShaderParameterInfo
{
    public string SemanticName;

    public unsafe void* SemanticNamePtr;

    public uint SemanticIndex;

    public uint Register;

    public ShaderSVType SystemValueType;

    public ShaderRegisterType ComponentType;

    public ShaderComponentMaskFlags Mask;

    public byte ReadWriteMask;

    public uint Stream;

    public ShaderMinPrecision MinPrecision;
}
