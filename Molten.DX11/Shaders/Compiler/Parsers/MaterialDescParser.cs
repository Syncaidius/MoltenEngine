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
        public MaterialDescParser(string nodeName) : base(nodeName) { }

        internal override NodeParseResult Parse(HlslShader shader, ShaderCompilerContext context, XmlNode node)
        {
            shader.Author = node.InnerText;
            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
