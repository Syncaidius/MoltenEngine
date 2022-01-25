using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class MaterialDescParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "description" };

        internal override NodeParseResult Parse(HlslFoundation foundation, HlslCompilerContext context, XmlNode node)
        {
            if (foundation is HlslShader shader)
            {
                shader.Description = node.InnerText;
                return new NodeParseResult(NodeParseResultType.Success);
            }
            else
            {
                return new NodeParseResult(NodeParseResultType.Ignored);
            }
        }
    }
}
