namespace Molten.Graphics;

public class ShaderStageCapabilities
{
    /// <summary>
    /// Gets or sets whether the shader stage is supported.
    /// </summary>
    public bool IsSupported { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the use of 10-bit-precision floating-point values is supported by the shader stage.
    /// </summary>
    public bool Float10 { get; set; }

    /// <summary>
    /// Gets or sets whether the use of 16-bit integer values is supported by the shader stage.
    /// </summary>
    public bool Int16 { get; set; }

    /// <summary>
    /// Gets or sets whether the use of 6-bit half-precision floating-point values is supported by the shader stage.
    /// </summary>
    public bool Float16 { get; set; }

    /// <summary>
    /// Gets or sets whether the use of 64-bit integer values is supported by the shader stage.
    /// </summary>
    public bool Int64 { get; set; }

    /// <summary>
    /// Gets or sets whether the use of 64-bit, double-precision floating-point values is supported by the shader stage.
    /// </summary>
    public bool Float64 { get; set; }

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
