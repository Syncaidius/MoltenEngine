namespace Molten.Graphics;

/// <summary>An entry-point tag parser used by <see cref="HlslShader"/> headers.</summary>
internal class ShaderGSParser : ShaderNodeParser
{
    public override ShaderNodeType NodeType => ShaderNodeType.Geometry;

    protected override void OnParse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node)
    {
        InitializeEntryPoint(passDef, context, node, ShaderType.Geometry);
    }
}
