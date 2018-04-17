using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class MaterialAuthorParser : ShaderNodeParser
    {
        public MaterialAuthorParser(string nodeName) : base(nodeName) { }

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is HlslShader shader)
            {
                shader.Description = string.IsNullOrWhiteSpace(node.InnerText) ? "Unknown" : node.InnerText;
                return new NodeParseResult(NodeParseResultType.Success);
            }
            else
            {
                return new NodeParseResult(NodeParseResultType.Ignored);
            }
        }
    }
}
