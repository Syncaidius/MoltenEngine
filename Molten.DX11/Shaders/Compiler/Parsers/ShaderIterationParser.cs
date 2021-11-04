using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class ShaderIterationParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "iterations" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            int val = 1;
            if (int.TryParse(node.InnerText, out val))
                foundation.Iterations = val;
            else
                context.Messages.Add($"Invalid iteration number format for {foundation.GetType().Name}. Should be an integer value.");

            if (string.IsNullOrWhiteSpace(node.InnerText))
                foundation.Name = "Unnamed Material";
            else
                foundation.Name = node.InnerText;

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
