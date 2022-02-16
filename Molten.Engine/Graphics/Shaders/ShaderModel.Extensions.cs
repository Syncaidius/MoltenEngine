using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>An extension class for the ShaderModel enumeration.</summary>
    public static class ShaderModelExtensions
    {
        /// <summary>Converts the shader model value to a usable profile string.</summary>
        /// <param name="model">The model value to convert.</param>
        /// <returns></returns>
        public static string ToProfile(this ShaderModel model, ShaderType profile, ShaderLanguage language)
        {
            switch(language)
            {
                default:
                case ShaderLanguage.Unknown: throw new Exception("Unsupported shader language.");
                case ShaderLanguage.Hlsl: return ToHlslProfile(model, profile);
                case ShaderLanguage.Glsl: return ToGlslProfile(model, profile);
                case ShaderLanguage.SpirV: return ToSpirVProfile(model, profile);
            }
        }

        public static string ToHlslProfile(this ShaderModel model, ShaderType profile)
        {
            string pString = "";

            switch (profile)
            {
                case ShaderType.ComputeShader:
                    pString += "cs_";
                    break;
                case ShaderType.DomainShader:
                    pString += "ds_";
                    break;
                case ShaderType.GeometryShader:
                    pString += "gs_";
                    break;
                case ShaderType.PixelShader:
                    pString += "ps_";
                    break;
                case ShaderType.VertexShader:
                    pString += "vs_";
                    break;
                case ShaderType.HullShader:
                    pString += "hs_";
                    break;

                case ShaderType.LibShader:
                    pString += "lib_";
                    break;

                default:
                    throw new InvalidOperationException("Cannot convert unknown shader model value.");
            }

            if (profile != ShaderType.Unknown)
                pString += model.ToString().Replace("Model", "");
            else
                throw new InvalidOperationException("Cannot convert unknown shader profile type.");

            return pString;
        }

        public static string ToGlslProfile(this ShaderModel model, ShaderType profile)
        {
            throw new NotImplementedException();
        }

        public static string ToSpirVProfile(this ShaderModel model, ShaderType profile)
        {
            throw new NotImplementedException();
        }
    }
}
