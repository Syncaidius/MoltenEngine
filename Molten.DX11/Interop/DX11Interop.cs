using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal static class DX11Interop
    {
        public static PipeStageType ToStageType(this ShaderType shaderType)
        {
            switch (shaderType)
            {
                case ShaderType.ComputeShader: return PipeStageType.ComputeShader;
                case ShaderType.DomainShader: return PipeStageType.DomainShader;
                case ShaderType.GeometryShader: return PipeStageType.GeometryShader;
                case ShaderType.HullShader: return PipeStageType.HullShader;
                case ShaderType.PixelShader: return PipeStageType.PixelShader;
                case ShaderType.VertexShader: return PipeStageType.VertexShader;

                default:
                case ShaderType.Unknown:
                    return PipeStageType.None;
            }
        }
    }
}
