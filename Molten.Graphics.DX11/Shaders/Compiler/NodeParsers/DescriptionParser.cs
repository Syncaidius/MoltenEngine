using System.Xml;

namespace Molten.Graphics
{
    internal class DescriptionParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Description;

        public override void Parse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            switch (foundation)
            {
                case HlslShader shader:
                    shader.Description = node.ValueType != ShaderHeaderValueType.None ? "Unknown" : node.Value;
                    break;

                case MaterialPass pass:
                    context.AddWarning($"Ignoring '{NodeType}' in material pass definition");
                    break;

                default:
                    context.AddWarning($"Ignoring '{NodeType}' in unsupported shader type '{foundation.GetType().Name}' definition");
                    break;
            }
        }
    }
}
