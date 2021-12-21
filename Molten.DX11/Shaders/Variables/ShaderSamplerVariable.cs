using Molten.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderSamplerVariable : IShaderValue
    {
        internal ShaderSampler Sampler { get; private set; }

        internal ShaderSamplerVariable(HlslShader shader)
        {
            Parent = shader;
            Sampler = shader.Device.SamplerBank.GetPreset(SamplerPreset.Default);
        }

        public IShader Parent { get; private set; }

        public string Name { get; set; }

        public object Value
        {
            get => Sampler;
            set
            {
                if (value != Sampler)
                {
                    ShaderSampler newSampler = value as ShaderSampler;
                    if (value != null && newSampler == null)
                        throw new InvalidOperationException("Cannot set non-DirectX 11 sampler on material in DX11 renderer.");
                    else
                        Sampler = newSampler;
                }
            }
        }
    }
}
