using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    public abstract class ShaderHeaderParser<CXT,S>
        where CXT : ShaderCompilerContext
        where S : IShader
    {
        public void ParserHeader(S foundation, ref string header, ShaderCompilerContext context)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(header);

            XmlNode rootNode = doc.ChildNodes[0];
            ParseNode(foundation, rootNode, context);
        }

        public void ParseNode(S foundation, XmlNode parentNode, ShaderCompilerContext context)
        {
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                string nodeName = node.Name.ToLower();
                ShaderNodeParser<CXT> parser = null;
                if (context.Compiler.NodeParsers.TryGetValue(nodeName, out parser))
                {
                    parser.Parse(foundation, context, node);
                }
                else
                {
                    if (parentNode.ParentNode != null)
                        context.AddWarning($"Ignoring unsupported {parentNode.ParentNode.Name} tag '{parentNode.Name}'");
                    else
                        context.AddWarning($"Ignoring unsupported root tag '{parentNode.Name}'");
                }
            }
        }
    }
}
