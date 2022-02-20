using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class ShaderNameParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Name;

        public override void Parse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, XmlNode node)
        {
            if (string.IsNullOrWhiteSpace(node.InnerText))
                foundation.Name = "Unnamed Material";
            else
                foundation.Name = node.InnerText;
        }
    }
}
