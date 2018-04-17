using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class ShaderPassParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "pass" };

        internal override NodeParseResult Parse(HlslFoundation shader, ShaderCompilerContext context, XmlNode node)
        {
            if (shader is Material material)
            {
                MaterialPass pass = new MaterialPass(material);
                context.Compiler.ParseNode(pass, node, context);

                // Add the pass once for each iteration it is meant to run.
                material.AddPass(pass);

                return new NodeParseResult(NodeParseResultType.Success);
            }
            else
            {
                return new NodeParseResult(NodeParseResultType.Ignored);
            }
        }
    }
}
