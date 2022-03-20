using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class ShaderHSStage : ContextShaderStage<ID3D11HullShader>
    {
        public ShaderHSStage(DeviceContextState state) : base(state, ShaderType.HullShader)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Context.Native->HSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** views)
        {
            Context.Native->HSSetShaderResources(startSlot, numViews, views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Context.Native->HSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Context.Native->HSSetShader((ID3D11HullShader*)shader, classInstances, numClassInstances);
        }
    }
}
