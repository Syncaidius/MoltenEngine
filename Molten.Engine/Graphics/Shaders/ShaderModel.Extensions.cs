namespace Molten.Graphics;

/// <summary>An extension class for the ShaderModel enumeration.</summary>
public static class ShaderModelExtensions
{
    /// <summary>Converts the shader model value to a usable profile string.</summary>
    /// <param name="model">The model value to convert.</param>
    /// <param name="type">The type of shader profile to define.</param>
    /// <returns></returns>
    public static string ToProfile(this ShaderModel model, ShaderStageType type)
    {
        string pString = "";

        switch (type)
        {
            case ShaderStageType.Compute:
                pString += "cs_";
                break;
            case ShaderStageType.Domain:
                pString += "ds_";
                break;
            case ShaderStageType.Geometry:
                pString += "gs_";
                break;
            case ShaderStageType.Pixel:
                pString += "ps_";
                break;
            case ShaderStageType.Vertex:
                pString += "vs_";
                break;
            case ShaderStageType.Hull:
                pString += "hs_";
                break;

            case ShaderStageType.Lib:
                pString += "lib_";
                break;

            default:
                throw new InvalidOperationException("Cannot convert unknown shader model value.");
        }

        if (type != ShaderStageType.Unknown)
            pString += model.ToString().Replace("Model", "");
        else
            throw new InvalidOperationException("Cannot convert unknown shader profile type.");

        return pString;
    }

    /// <summary>
    /// Clamps the <see cref="ShaderModel"/> to the specified minimum and/or maximum models.
    /// </summary>
    /// <param name="modelValue"></param>
    /// <param name="minModel"></param>
    /// <param name="maxModel"></param>
    /// <returns></returns>
    public static ShaderModel Clamp(this ShaderModel modelValue, ShaderModel minModel, ShaderModel maxModel)
    {
        uint min = (uint)minModel;
        uint max = (uint)maxModel;
        uint model = Math.Clamp((uint)modelValue, min, max);

        return (ShaderModel)model;
    }
}
