namespace Molten.Graphics
{
    public class ShaderSamplerVariable : IShaderValue
    {
        internal ShaderSamplerDX11 Sampler { get; private set; }

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
                    ShaderSamplerDX11 newSampler = value as ShaderSamplerDX11;
                    if (value != null && newSampler == null)
                        throw new InvalidOperationException("Cannot set non-DirectX 11 sampler on material in DX11 renderer.");
                    else
                        Sampler = newSampler;
                }
            }
        }
    }
}
