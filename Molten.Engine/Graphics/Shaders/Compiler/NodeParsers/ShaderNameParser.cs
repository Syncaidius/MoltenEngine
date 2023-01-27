namespace Molten.Graphics
{
    internal class ShaderNameParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Name;

        public override Type[] TypeFilter => null;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            foundation.Name = string.IsNullOrWhiteSpace(node.Value) ? "Unnamed Material" : node.Value;
        }
    }
}
