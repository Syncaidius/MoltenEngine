using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>An extension class for the ShaderModel enumeration.</summary>
    public static class ShaderModelExtensionsDX11
    {
        /// <summary>Converts the shader model value to a usable profile string.</summary>
        /// <param name="model">The model value to convert.</param>
        /// <returns></returns>
        public static string ToProfile(this ShaderModel model, ShaderType profile)
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
            }

            switch (model)
            {
                case ShaderModel.Model3_0:
                    pString += "3_0";
                    break;
                case ShaderModel.Model4_0:
                    pString += "4_0";
                    break;
                case ShaderModel.Model4_1:
                    pString += "4_1";
                    break;
                case ShaderModel.Model5_0:
                    pString += "5_0";
                    break;
                default:
                    pString += "4_0";
                    break;
            }

            return pString;
        }
    }
}
