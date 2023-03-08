namespace Molten.Graphics
{
    public class ShaderSamplerVariable : IShaderValue
    {
        public ShaderSampler Sampler { get; private set; }

        public ShaderSamplerVariable(HlslShader shader)
        {
            Parent = shader;
            ShaderSamplerParameters defaultParams = new ShaderSamplerParameters();
            defaultParams.ApplyPreset(SamplerPreset.Default);
            Sampler = shader.Device.CreateSampler(ref defaultParams);
        }

        public HlslShader Parent { get; private set; }

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
                        throw new InvalidOperationException("Cannot set non-sampler object on a sampler variable.");
                    else
                        Sampler = newSampler;
                }
            }
        }
    }
}
