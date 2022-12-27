using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderGSStage : ContextShaderStage<ID3D11GeometryShader>
    {
        public ShaderGSStage(DeviceContextState state) : base(state, ShaderType.Geometry)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Context.Native->GSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views)
        {
            Context.Native->GSSetShaderResources(startSlot, numViews, (ID3D11ShaderResourceView**)views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Context.Native->GSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Context.Native->GSSetShader((ID3D11GeometryShader*)shader, classInstances, numClassInstances);
        }
    }
}
