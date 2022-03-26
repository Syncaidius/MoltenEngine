using System.Xml;

namespace Molten.Graphics
{
    internal class ShaderNameParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Name;

        public override Type[] TypeFilter => null;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            foundation.Name = string.IsNullOrWhiteSpace(node.Value) ? "Unnamed Material" : node.Value;
        }
    }
}
