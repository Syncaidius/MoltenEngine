namespace Molten.Graphics
{
    public class ShaderSamplerVariable : IShaderValue
    {
        public GraphicsSampler Sampler { get; private set; }

        public ShaderSamplerVariable(HlslShader shader)
        {
            Parent = shader;
            GraphicsSamplerParameters defaultParams = new GraphicsSamplerParameters();
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
                    GraphicsSampler newSampler = value as GraphicsSampler;
                    if (value != null && newSampler == null)
                        throw new InvalidOperationException("Cannot set non-sampler object on a sampler variable.");
                    else
                        Sampler = newSampler;
                }
            }
        }
    }
}
