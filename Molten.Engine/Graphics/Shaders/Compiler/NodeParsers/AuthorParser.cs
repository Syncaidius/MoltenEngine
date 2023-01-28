namespace Molten.Graphics
{
    internal class AuthorParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Author;

        public override Type[] TypeFilter { get; } = { typeof(IShader) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            (foundation as IShader).Author = node.ValueType != ShaderHeaderValueType.None ? "Unknown" : node.Value;
        }
    }
}
