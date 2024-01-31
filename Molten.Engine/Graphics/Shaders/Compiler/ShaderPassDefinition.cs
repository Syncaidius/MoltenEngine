using Newtonsoft.Json;

namespace Molten.Graphics;
public class ShaderPassDefinition
{
    public string Name { get; set; }

    public int Iterations { get; set; } = 1;

    [JsonProperty("entry")]
    public ShaderEntryPointDefinition Entry { get; set; } = new ShaderEntryPointDefinition();

    public ShaderPassParameters Parameters = new ShaderPassParameters(GraphicsStatePreset.Default, PrimitiveTopology.Triangle);

    public ShaderSamplerParameters[] Samplers = new ShaderSamplerParameters[0];
}
