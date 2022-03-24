using System.Xml;

namespace Molten.Graphics
{
    internal class AuthorParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Author;

        public override Type[] TypeFilter { get; } = { typeof(HlslShader) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            (foundation as HlslShader).Author = node.ValueType != ShaderHeaderValueType.None ? "Unknown" : node.Value;
        }
    }
}
