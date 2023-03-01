namespace Molten.Graphics
{
    internal class DescriptionParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Description;

        public override Type[] TypeFilter { get; } = { typeof(HlslShader) };

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string desc))
                (foundation as HlslShader).Description = desc;
            else
                (foundation as HlslShader).Description = "Unknown";
        }
    }
}
