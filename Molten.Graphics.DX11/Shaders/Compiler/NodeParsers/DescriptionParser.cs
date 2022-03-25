using System.Xml;

namespace Molten.Graphics
{
    internal class DescriptionParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Description;

        public override Type[] TypeFilter { get; } = { typeof(HlslShader) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            (foundation as HlslShader).Description = node.ValueType != ShaderHeaderValueType.None ? "Unknown" : node.Value;
        }
    }
}
