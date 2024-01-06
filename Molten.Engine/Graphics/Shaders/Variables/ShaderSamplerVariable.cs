namespace Molten.Graphics;

public class ShaderSamplerVariable : ShaderVariable
{
    public ShaderSampler Sampler { get; private set; }

    protected override void Initialize()
    {
        ShaderSamplerParameters defaultParams = new ShaderSamplerParameters(SamplerPreset.Default);
        Sampler = Parent.Device.CreateSampler(ref defaultParams);
    }

    /// <inheritdoc/>
    public override object Value
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
