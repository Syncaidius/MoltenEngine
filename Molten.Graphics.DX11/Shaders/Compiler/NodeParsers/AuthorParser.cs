using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class AuthorParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Author;

        public override void Parse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, XmlNode node)
        {
            switch (foundation)
            {
                case HlslShader shader:
                    shader.Description = string.IsNullOrWhiteSpace(node.InnerText) ? "Unknown" : node.InnerText;
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
