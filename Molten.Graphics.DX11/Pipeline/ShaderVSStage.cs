using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal class ShaderVSStage : ShaderStageDX11
    {
        public ShaderVSStage(GraphicsQueueDX11 queue) : base(queue, ShaderType.Vertex)
        {
        }

        internal override unsafe void SetConstantBuffers(uint numBuffers, ID3D11Buffer** buffers)
        {
            Cmd.Ptr->VSSetConstantBuffers(0, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint numViews, ID3D11ShaderResourceView1** views)
        {
            Cmd.Ptr->VSSetShaderResources(0, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint numSamplers, ID3D11SamplerState** states)
        {
            Cmd.Ptr->VSSetSamplers(0, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Cmd.Ptr->VSSetShader((ID3D11VertexShader*)shader, classInstances, numClassInstances);
        }
    }
}
