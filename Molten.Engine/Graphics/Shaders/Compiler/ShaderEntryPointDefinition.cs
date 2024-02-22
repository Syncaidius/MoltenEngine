using Newtonsoft.Json;

namespace Molten.Graphics;
public class ShaderEntryPointDefinition
{
    string _vs;
    string _ps;
    string _gs;
    string _hs;
    string _ds;
    string _cs;

    /// <summary>
    /// Gets or sets the vertex shader entry point.
    /// </summary>
    [JsonProperty("vertex")]
    public string Vertex
    {
        get => _vs;
        set
        {
            _vs = value;
            Points[ShaderStageType.Vertex] = value;
        }
    }

    /// <summary>
    /// Gets or sets the vertex shader entry point. This is identical to <see cref="Fragment"/>.
    /// </summary>
    [JsonProperty("pixel")]
    public string Pixel
    {
        get => _ps;
        set
        {
            _ps = value;
            Points[ShaderStageType.Pixel] = value;
        }
    }

    /// <summary>
    /// Gets or sets the fragment shader entry point. This is identical to <see cref="Pixel"/>.
    /// </summary>
    [JsonProperty("fragment")]
    public string Fragment
    {
        get => _ps;
        set
        {
            _ps = value;
            Points[ShaderStageType.Pixel] = value;
        }
    }

    /// <summary>
    /// Gets or sets the geometry shader entry point.
    /// </summary>
    [JsonProperty("geometry")]
    public string Geometry
    {
        get => _gs;
        set
        {
            _gs = value;
            Points[ShaderStageType.Geometry] = value;
        }
    }

    /// <summary>
    /// Gets or sets the hull shader entry point.
    /// </summary>
    [JsonProperty("hull")]
    public string Hull
    {
        get => _hs;
        set
        {
            _hs = value;
            Points[ShaderStageType.Hull] = value;
        }
    }

    [JsonProperty("domain")]
    public string Domain
    {
        get => _ds;
        set
        {
            _ds = value;
            Points[ShaderStageType.Domain] = value;
        }
    }

    [JsonProperty("compute")]
    public string Compute
    {
        get => _cs;
        set
        {
            _cs = value;
            Points[ShaderStageType.Compute] = value;
        }
    }

    [JsonIgnore]
    public Dictionary<ShaderStageType, string> Points { get; } = new Dictionary<ShaderStageType, string>();
}