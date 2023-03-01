namespace Molten.Graphics
{
    internal class ShaderNameParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Name;

        public override Type[] TypeFilter => null;

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string name))
                foundation.Name = name;
        }
    }
}
