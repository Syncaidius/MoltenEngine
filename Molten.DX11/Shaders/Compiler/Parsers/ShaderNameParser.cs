using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class ShaderNameParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] {"name"};

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (string.IsNullOrWhiteSpace(node.InnerText))
                foundation.Name = "Unnamed Material";
            else
                foundation.Name = node.InnerText;

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
