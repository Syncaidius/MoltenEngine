using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class CSShaderStage : ContextShaderStage
    {
        public CSShaderStage(DeviceContext context) : base(context, ShaderType.ComputeShader)
        {
        }

        internal override unsafe void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** states)
        {
            throw new NotImplementedException();
        }

        internal override unsafe void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** states)
        {
            throw new NotImplementedException();
        }

        internal override unsafe void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states)
        {
            throw new NotImplementedException();
        }

        internal override unsafe void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances)
        {
            Context.Native->CSSetShader((ID3D11ComputeShader*)shader, classInstances, numClassInstances);
        }

        internal unsafe void SetUnorderedAccessViews(uint startSlot, uint numUAVs, ID3D11UnorderedAccessView** ppUnorderedAccessViews, ref uint* pUAVInitialCounts)
        {
            Context.Native->CSSetUnorderedAccessViews(startSlot, numUAVs, ppUnorderedAccessViews, pUAVInitialCounts);
        }
    }
}
