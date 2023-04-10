using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal class ShaderVSStage : ShaderStageDX11
    {
        public ShaderVSStage(GraphicsQueueDX11 queue) : base(queue, ShaderType.Vertex)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Cmd.Ptr->VSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Cmd.Ptr->VSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Cmd.Ptr->VSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Cmd.Ptr->VSSetShader((ID3D11VertexShader*)shader, classInstances, numClassInstances);
        }
    }
}
