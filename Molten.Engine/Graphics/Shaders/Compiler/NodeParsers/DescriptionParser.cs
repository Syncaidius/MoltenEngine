namespace Molten.Graphics
{
    internal class DescriptionParser : ShaderNodeParser<HlslShader>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Description;

        protected override void OnParse(HlslShader shader, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string desc))
                shader.Description = desc;
            else
                shader.Description = "Unknown";
        }
    }
}
