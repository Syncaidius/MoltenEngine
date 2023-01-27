using System.Xml;

namespace Molten.Graphics
{
    internal class DescriptionParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Description;

        public override Type[] TypeFilter { get; } = { typeof(IShader) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            (foundation as IShader).Description = node.ValueType != ShaderHeaderValueType.None ? "Unknown" : node.Value;
        }
    }
}
