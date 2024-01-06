namespace Molten.Graphics;

internal class DescriptionParser : ShaderNodeParser
{
    public override ShaderNodeType NodeType => ShaderNodeType.Description;

    protected override void OnParse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node)
    {
        if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string desc))
            def.Description = desc;
        else
            def.Description = "Unknown";
    }
}
