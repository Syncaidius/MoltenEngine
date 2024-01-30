namespace Molten.Graphics;

public class ShaderDefinition
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Author { get; set; }

    public string Version { get; set; } = "1.0.0";

    public string File { get; set; }

    public ShaderPassDefinition[] Passes { get; set; } = new ShaderPassDefinition[0];
}

public class ShaderPassDefinition
{
    public string Name { get; set; }

    public int Iterations { get; set; } = 1;

    public ShaderEntryPointDefinition Entry { get; set; } = new ShaderEntryPointDefinition();

    public ShaderPassParameters Parameters = new ShaderPassParameters(GraphicsStatePreset.Default, PrimitiveTopology.Triangle);

    public ShaderSamplerParameters[] Samplers = new ShaderSamplerParameters[0];
}
