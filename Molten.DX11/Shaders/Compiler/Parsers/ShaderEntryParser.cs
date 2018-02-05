using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderEntryParser : ShaderNodeParser
    {
        public ShaderEntryParser(string nodeName) : base(nodeName) { }

        internal override NodeParseResult Parse(HlslShader shader, XmlNode node)
        {
            if (!(shader is ComputeTask))
                return new NodeParseResult(NodeParseResultType.Ignored);

            ComputeTask cTask = shader as ComputeTask;
            if (string.IsNullOrWhiteSpace(node.InnerText))
                return new NodeParseResult(NodeParseResultType.Error, "Compute task <entry> tag is missing or empty.");
            else
                cTask.Composition.EntryPoint = node.InnerText;

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
