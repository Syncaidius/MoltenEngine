namespace Molten.Graphics
{
    internal class AuthorParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Author;

        public override Type[] TypeFilter { get; } = { typeof(HlslShader) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if(node.Values.TryGetValue(ShaderHeaderValueType.Value, out string author))
                (foundation as HlslShader).Author = author;
            else
                (foundation as HlslShader).Author = "Unknown";
        }
    }
}
