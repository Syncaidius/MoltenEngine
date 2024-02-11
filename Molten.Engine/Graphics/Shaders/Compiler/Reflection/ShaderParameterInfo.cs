using Silk.NET.Core.Native;

namespace Molten.Graphics;

/// <summary>
/// Information about a shader's input or output parameters.
/// </summary>
public class ShaderParameterInfo : IDisposable
{
    /// <summary>
    /// A per-parameter string that identifies how the data will be used. For more info, see Semantics.
    /// </summary>
    public string SemanticName;

    /// <summary>
    /// Contains the native API string pointer for <see cref="SemanticName"/>. If null, no native pointer was provided.
    /// </summary>
    public unsafe void* SemanticNamePtr;

    /// <summary>
    /// Semantic index that modifies the semantic. Used to differentiate different parameters that use the same semantic.
    /// </summary>
    public uint SemanticIndex;

    /// <summary>
    /// The register that will contain this variable's data.
    /// </summary>
    public uint Register;

    /// <summary>
    /// A value that identifies a predefined string that determines the functionality of certain pipeline stages.
    /// </summary>
    public ShaderSVType SystemValueType;

    /// <summary>
    /// A <see cref="ShaderRegisterType"/> that identifies the per-component-data type that is stored in a register. 
    /// Each register can store up to four-components of data.
    /// </summary>
    public ShaderRegisterType ComponentType;

    /// <summary>
    /// Mask which indicates which components of a register are used.
    /// </summary>
    public ShaderComponentMaskFlags Mask;

    /// <summary>
    /// Mask which indicates whether a given component is never written (if the signature is an output signature) or always read (if the signature is an input signature).
    /// </summary>
    public ShaderComponentMaskFlags ReadWriteMask;

    /// <summary>
    /// Indicates which stream the geometry shader is using for the signature parameter.
    /// </summary>
    public uint Stream;

    /// <summary>
    /// A value that indicates the minimum desired interpolation precision.
    /// </summary>
    public ShaderMinPrecision MinPrecision;

    public unsafe void Dispose()
    {
        SilkMarshal.FreeString((nint)SemanticNamePtr);
    }
}
