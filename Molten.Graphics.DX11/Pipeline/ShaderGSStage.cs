using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

internal class ShaderGSStage : ShaderStageDX11
{
    public ShaderGSStage(GraphicsQueueDX11 queue) : base(queue, ShaderType.Geometry) { }

    internal override unsafe void SetConstantBuffers(uint numBuffers, ID3D11Buffer** buffers)
    {
        Cmd.Ptr->GSSetConstantBuffers(0, numBuffers, buffers);
    }

    internal override unsafe void SetResources(uint numViews, ID3D11ShaderResourceView1** views)
    {
        Cmd.Ptr->GSSetShaderResources(0, numViews, (ID3D11ShaderResourceView**)views);
    }

    internal override unsafe void SetSamplers(uint numSamplers, ID3D11SamplerState** states)
    {
        Cmd.Ptr->GSSetSamplers(0, numSamplers, states);
    }

    internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
    {
        Cmd.Ptr->GSSetShader((ID3D11GeometryShader*)shader, classInstances, numClassInstances);
    }
}
