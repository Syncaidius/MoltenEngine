using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderDSStage : ContextShaderStage<ID3D11DomainShader>
    {
        public ShaderDSStage(CommandQueueDX11 queue) : base(queue, ShaderType.Domain)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Context.Native->DSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Context.Native->DSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Context.Native->DSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Context.Native->DSSetShader((ID3D11DomainShader*)shader, classInstances, numClassInstances);
        }
    }
}
