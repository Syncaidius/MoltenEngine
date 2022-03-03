using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe abstract class ContextShaderStage
    {
        internal ContextShaderStage(DeviceContext context, ShaderType type)
        {
            Context = context;
            Type = type;
        }

        internal abstract void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states);

        internal abstract void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** views);

        internal abstract void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers);

        internal abstract void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances);

        internal DeviceContext Context { get; }

        internal ShaderType Type { get; }
    }
}
