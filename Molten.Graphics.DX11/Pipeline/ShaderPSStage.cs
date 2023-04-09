using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderPSStage : ShaderStageDX11
    {
        public ShaderPSStage(GraphicsQueueDX11 queue) : base(queue, ShaderType.Pixel)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Cmd.Native->PSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Cmd.Native->PSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Cmd.Native->PSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Cmd.Native->PSSetShader((ID3D11PixelShader*)shader, classInstances, numClassInstances);
        }
    }
}
