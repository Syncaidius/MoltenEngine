namespace Molten.Graphics;

public abstract class ShaderSampler : GraphicsObject, IEquatable<ShaderSampler>, IEquatable<ShaderSamplerParameters>
{
    ShaderSamplerParameters _parameters;

    protected ShaderSampler(GraphicsDevice device, ref ShaderSamplerParameters parameters) : 
        base(device)
    {
        IsComparisonSampler = parameters.IsComparison;
        _parameters = parameters;
    }

    public override bool Equals(object obj) => obj switch
    {
        ShaderSampler other => _parameters.Equals(other._parameters),
        ShaderSamplerParameters other => _parameters.Equals(other),
        _ => base.Equals(obj),
    };

    public bool Equals(ShaderSampler other)
    {
        return _parameters.Equals(other._parameters);
    }

    public bool Equals(ShaderSamplerParameters other)
    {
        return _parameters.Equals(other);
    }

    /// <summary>Gets whether or not the sampler a comparison sampler. This is determined by the <see cref="Filter"/> mode.</summary>
    public bool IsComparisonSampler { get; private set; }

}
