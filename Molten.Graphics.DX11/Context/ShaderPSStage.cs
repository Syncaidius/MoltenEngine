using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ShaderPSStage : ContextShaderStage<ID3D11PixelShader>
    {
        public ShaderPSStage(DeviceContextState state) : base(state, ShaderType.PixelShader)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers)
        {
            Context.Native->PSSetConstantBuffers(startSlot, numBuffers, buffers);
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** views)
        {
            Context.Native->PSSetShaderResources(startSlot, numViews, views);
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            Context.Native->PSSetSamplers(startSlot, numSamplers, states);
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Context.Native->PSSetShader((ID3D11PixelShader*)shader, classInstances, numClassInstances);
        }
    }
}
