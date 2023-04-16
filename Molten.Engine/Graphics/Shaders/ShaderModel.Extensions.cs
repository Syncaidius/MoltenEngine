namespace Molten.Graphics
{
    /// <summary>An extension class for the ShaderModel enumeration.</summary>
    public static class ShaderModelExtensions
    {
        /// <summary>Converts the shader model value to a usable profile string.</summary>
        /// <param name="model">The model value to convert.</param>
        /// <param name="type">The type of shader profile to define.</param>
        /// <returns></returns>
        public static string ToProfile(this ShaderModel model, ShaderType type)
        {
            string pString = "";

            switch (type)
            {
                case ShaderType.Compute:
                    pString += "cs_";
                    break;
                case ShaderType.Domain:
                    pString += "ds_";
                    break;
                case ShaderType.Geometry:
                    pString += "gs_";
                    break;
                case ShaderType.Pixel:
                    pString += "ps_";
                    break;
                case ShaderType.Vertex:
                    pString += "vs_";
                    break;
                case ShaderType.Hull:
                    pString += "hs_";
                    break;

                case ShaderType.Lib:
                    pString += "lib_";
                    break;

                default:
                    throw new InvalidOperationException("Cannot convert unknown shader model value.");
            }

            if (type != ShaderType.Unknown)
                pString += model.ToString().Replace("Model", "");
            else
                throw new InvalidOperationException("Cannot convert unknown shader profile type.");

            return pString;
        }
    }
}
