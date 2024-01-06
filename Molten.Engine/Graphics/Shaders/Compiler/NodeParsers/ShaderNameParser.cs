namespace Molten.Graphics;

internal class ShaderNameParser : ShaderNodeParser
{
    public override ShaderNodeType NodeType => ShaderNodeType.Name;

    protected override void OnParse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node)
    {
        if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string name))
        {
            if (passDef != null)
                passDef.Name = name;
            else
                def.Name = name;
        }
    }
}
