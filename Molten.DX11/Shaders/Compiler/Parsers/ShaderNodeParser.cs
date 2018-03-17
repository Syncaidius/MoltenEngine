using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal abstract class ShaderNodeParser
    {
        internal string NodeName { get; private set; }

        public ShaderNodeParser(string nodeName)
        {
            NodeName = nodeName;
        }

        internal abstract NodeParseResult Parse(HlslShader shader, ShaderCompilerContext context, XmlNode node);
    }
}
