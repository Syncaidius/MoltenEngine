using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderGSStage : ShaderStageDX11
    {
        public ShaderGSStage(GraphicsQueueDX11 queue) : base(queue, ShaderType.Geometry)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Cmd.Native->GSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Cmd.Native->GSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Cmd.Native->GSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Cmd.Native->GSSetShader((ID3D11GeometryShader*)shader, classInstances, numClassInstances);
        }
    }
}
