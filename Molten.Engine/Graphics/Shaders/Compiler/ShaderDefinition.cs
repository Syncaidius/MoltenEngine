namespace Molten.Graphics;

internal class ShaderDefinition
{
    public string Name;

    public string Description;

    public string Author;

    internal ShaderPassDefinition AddPass()
    {
        ShaderPassDefinition pass = new ShaderPassDefinition();
        Passes.Add(pass);
        return pass;
    }

    public List<ShaderPassDefinition> Passes { get; } = new List<ShaderPassDefinition>();
}

internal class ShaderPassDefinition
{
    public string Name;

    public Dictionary<ShaderType, string> EntryPoints { get; } = new Dictionary<ShaderType, string>();

    public ShaderPassParameters Parameters = new ShaderPassParameters(GraphicsStatePreset.Default, PrimitiveTopology.Triangle);

    public ShaderSamplerParameters[] Samplers = new ShaderSamplerParameters[0];

    public int Iterations = 1;
}
