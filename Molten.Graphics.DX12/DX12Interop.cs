using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    internal static class DX12Interop
    {
        internal static D3DShaderModel ToApi(this ShaderModel model)
        {
            switch (model)
            {
                default:
                case ShaderModel.Model5_0:
                case ShaderModel.Model5_1:
                    return D3DShaderModel.ShaderModel51;

                case ShaderModel.Model6_0: return D3DShaderModel.ShaderModel60;
                case ShaderModel.Model6_1: return D3DShaderModel.ShaderModel61;
                case ShaderModel.Model6_2: return D3DShaderModel.ShaderModel62;
                case ShaderModel.Model6_3: return D3DShaderModel.ShaderModel63;
                case ShaderModel.Model6_4: return D3DShaderModel.ShaderModel64;
                case ShaderModel.Model6_5: return D3DShaderModel.ShaderModel65;
                case ShaderModel.Model6_6: return D3DShaderModel.ShaderModel66;
                case ShaderModel.Model6_7: return D3DShaderModel.ShaderModel67;
            }
        }

        internal static ShaderModel FromApi(this D3DShaderModel model)
        {
            switch (model)
            {
                default:
                case D3DShaderModel.ShaderModel51:
                    return ShaderModel.Model5_1;

                case D3DShaderModel.ShaderModel60: return ShaderModel.Model6_0;
                case D3DShaderModel.ShaderModel61: return ShaderModel.Model6_1;
                case D3DShaderModel.ShaderModel62: return ShaderModel.Model6_2;
                case D3DShaderModel.ShaderModel63: return ShaderModel.Model6_3;
                case D3DShaderModel.ShaderModel64: return ShaderModel.Model6_4;
                case D3DShaderModel.ShaderModel65: return ShaderModel.Model6_5;
                case D3DShaderModel.ShaderModel66: return ShaderModel.Model6_6;
                case D3DShaderModel.ShaderModel67: return ShaderModel.Model6_7;
            }
        }
    }
}
