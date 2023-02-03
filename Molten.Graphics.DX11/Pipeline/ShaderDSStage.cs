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
            Cmd.Native->DSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Cmd.Native->DSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Cmd.Native->DSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Cmd.Native->DSSetShader((ID3D11DomainShader*)shader, classInstances, numClassInstances);
        }

        protected override unsafe void GetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** ptrViews)
        {
            Cmd.Native->DSGetShaderResources(startSlot, numViews, ptrViews);
        }

        protected override unsafe void GetShader(void** shader, ID3D11ClassInstance** classInstances, uint* numClassInstances)
        {
            Cmd.Native->DSGetShader((ID3D11DomainShader**)shader, classInstances, numClassInstances);
        }
    }
}
