namespace Molten.Graphics
{
    internal class AuthorParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Author;

        protected override void OnParse(ShaderDefinition header, ShaderPassDefinition passHeader, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if(node.Values.TryGetValue(ShaderHeaderValueType.Value, out string author))
                header.Author = author;
            else
                header.Author = "Unknown";
        }
    }
}
