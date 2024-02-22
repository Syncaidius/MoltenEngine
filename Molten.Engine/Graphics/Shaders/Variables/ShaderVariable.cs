namespace Molten.Graphics;

public abstract class ShaderVariable
{
    /// <summary>
    /// Gets the name of the variable.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Shader"/> that the current <see cref="ShaderVariable"/> belongs to.
    /// </summary>
    public Shader Parent { get; internal set; }

    /// <summary>
    /// Gets or sets the value of the current <see cref="ShaderVariable"/>.
    /// </summary>
    public abstract object Value { get; set; }
}
