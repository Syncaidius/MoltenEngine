using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

internal class ShaderPSStage : ShaderStageDX11
{
    public ShaderPSStage(GraphicsQueueDX11 queue) : base(queue, ShaderStageType.Pixel) { }

    internal override unsafe void SetConstantBuffers(uint numBuffers, ID3D11Buffer** buffers)
    {
        Cmd.Ptr->PSSetConstantBuffers(0, numBuffers, buffers);
    }

    internal override unsafe void SetResources(uint numViews, ID3D11ShaderResourceView1** views)
    {
        Cmd.Ptr->PSSetShaderResources(0, numViews, (ID3D11ShaderResourceView**)views);
    }

    internal override unsafe void SetSamplers(uint numSamplers, ID3D11SamplerState** states)
    {
        Cmd.Ptr->PSSetSamplers(0, numSamplers, states);
    }

    internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
    {
        Cmd.Ptr->PSSetShader((ID3D11PixelShader*)shader, classInstances, numClassInstances);
    }
}
