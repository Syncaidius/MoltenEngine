namespace Molten.Graphics;

public class ShaderStageCapabilities
{
    /// <summary>
    /// Gets or sets the flags that indicate support for various shader stage features.
    /// </summary>
    public ShaderCapabilityFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of input slots/registers for the shader stage.
    /// </summary>
    public uint MaxInRegisters { get; set; }

    /// <summary>
    /// Gets the maximum number of input buffers, textures and other resources supported by the shader stage.
    /// </summary>
    public uint MaxInResources { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of output slots/registers for the shader stage.
    /// </summary>
    public uint MaxOutRegisters { get; set; }

    /// <summary>
    /// Gets the maximum number of output buffers or surfaces supported by the shader stage.
    /// </summary>
    public uint MaxOutputTargets { get; set; }

    /// <summary>
    /// Gets or sets the number of supported unordered access resources. 
    /// <para>In DirectX 11 this would be Unordered-Access-View (UAV) based resources.</para>
    /// </summary>
    public uint MaxUnorderedAccessSlots { get; set; }

    public virtual bool Compatible(ShaderStageCapabilities other)
    {
        // TODO compare current to other. Current must have at least everything 'other' specifies.

        return true;
    }
}
