using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderHSStage : ShaderStageDX11
    {
        public ShaderHSStage(GraphicsQueueDX11 queue) : base(queue, ShaderType.Hull)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Cmd.Ptr->HSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Cmd.Ptr->HSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Cmd.Ptr->HSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Cmd.Ptr->HSSetShader((ID3D11HullShader*)shader, classInstances, numClassInstances);
        }
    }
}
