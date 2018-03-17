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
        public ShaderNameParser(string nodeName) : base(nodeName) { }

        internal override NodeParseResult Parse(HlslShader shader, ShaderCompilerContext context, XmlNode node)
        {
            if (string.IsNullOrWhiteSpace(node.InnerText))
                shader.Name = "Unnamed Material";
            else
                shader.Name = node.InnerText;

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
