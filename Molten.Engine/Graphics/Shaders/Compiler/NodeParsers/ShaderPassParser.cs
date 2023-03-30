namespace Molten.Graphics
{
    internal class ShaderPassParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pass;

        protected override void OnParse(ShaderDefinition header, ShaderPassDefinition passHeader, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            passHeader = header.AddPass();
            context.Compiler.ParseNode(header, passHeader, node.OriginalNode, context);
        }
    }
}
