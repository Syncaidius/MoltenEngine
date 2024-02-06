namespace Molten.Graphics;

public abstract class ShaderResourceVariable : ShaderVariable
{
    GraphicsResource _resource;

    protected abstract bool ValidateResource(GraphicsResource value);

    /// <summary>Gets the resource bound to the variable.</summary>
    public GraphicsResource Resource => _resource;

    /// <summary>
    /// Gets or sets the value of the resource variable.
    /// </summary>
    public override object Value
    {
        get => _resource;
        set
        {
            if (value != _resource)
            {
                if (value != null)
                {
                    if (value is GraphicsResource res && ValidateResource(res))
                    {
                        if (ExpectedFormat != GraphicsFormat.Unknown && res.ResourceFormat != ExpectedFormat)
                            Parent.Device.Log.Error($"Resource format mismatch. Expected '{ExpectedFormat}' but got '{res.ResourceFormat}'.");
                        else
                            _resource = res;
                    }
                    else
                    {
                        Parent.Device.Log.Error($"Cannot set non-resource '{value.GetType().Name}' object on resource variable '{Name}'");
                    }
                }
                else
                {
                    _resource = null;
                }
            }
        }
    }

    /// <summary>
    /// Gets the expected format of the <see cref="Value"/> resource.
    /// </summary>
    public GraphicsFormat ExpectedFormat { get; internal set; }
}

public class ShaderResourceVariable<T> : ShaderResourceVariable
{
    protected override bool ValidateResource(GraphicsResource value)
    {
        return value is T;
    }
}
