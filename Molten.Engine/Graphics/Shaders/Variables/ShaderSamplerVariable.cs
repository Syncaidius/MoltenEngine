namespace Molten.Graphics;

public class ShaderSamplerVariable : ShaderVariable
{
    ShaderSampler _sampler;

    /// <summary>
    /// Gets or sets whether or not the shader sampler variable is immutable.
    /// </summary>
    public bool IsImmutable { get; internal set; }

    public override object Value
    {
        get => _sampler;
        set
        {
            if (IsImmutable)
                Parent.Device.Log.Error($"Unable to set the value of an immutable shader sampler variable '{Name}'.");
            else
                _sampler = (ShaderSampler)value;
        }
    }
}
