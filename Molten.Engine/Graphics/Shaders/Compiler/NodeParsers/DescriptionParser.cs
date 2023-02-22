namespace Molten.Graphics
{
    internal class DescriptionParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Description;

        public override Type[] TypeFilter { get; } = { typeof(IShader) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string desc))
                (foundation as IShader).Description = desc;
            else
                (foundation as IShader).Description = "Unknown";
        }
    }
}
