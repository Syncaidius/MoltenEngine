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

        internal override NodeParseResult Parse(HlslShader shader, XmlNode node)
        {
            shader.Description = string.IsNullOrWhiteSpace(node.InnerText) ? "Unknown" : node.InnerText;
            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
