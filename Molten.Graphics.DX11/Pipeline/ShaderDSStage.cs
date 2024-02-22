using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

internal class ShaderDSStage : ShaderStageDX11
{
    public ShaderDSStage(GraphicsQueueDX11 queue) : base(queue, ShaderStageType.Domain) { }

    internal override unsafe void SetConstantBuffers(uint numBuffers, ID3D11Buffer** buffers)
    {
        Cmd.Ptr->DSSetConstantBuffers(0, numBuffers, buffers);
    }

    internal override unsafe void SetResources(uint numViews, ID3D11ShaderResourceView1** views)
    {
        Cmd.Ptr->DSSetShaderResources(0, numViews, (ID3D11ShaderResourceView**)views);
    }

    internal override unsafe void SetSamplers(uint numSamplers, ID3D11SamplerState** states)
    {
        Cmd.Ptr->DSSetSamplers(0, numSamplers, states);
    }

    internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
    {
        Cmd.Ptr->DSSetShader((ID3D11DomainShader*)shader, classInstances, numClassInstances);
    }
}
