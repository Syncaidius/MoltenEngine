using System.Xml;

namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderHSParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Hull;

        public override void Parse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, XmlNode node)
        {
            if (foundation is ComputeTask)
            {
                context.AddWarning($"Ignoring {NodeType} in compute task definition");
                return;
            }

            switch (foundation)
            {
                case Material material:
                    material.DefaultHSEntryPoint = node.InnerText;
                    break;

                case MaterialPass pass:
                    pass.HullShader .EntryPoint = node.InnerText;
                    break;

                default:
                    context.AddWarning($"Ignoring '{NodeType}' in unsupported shader type '{foundation.GetType().Name}' definition");
                    break;
            }
        }
    }
}
