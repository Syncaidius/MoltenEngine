using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderVSParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "vertex" };

        internal override NodeParseResult Parse(HlslFoundation foundation, HlslCompilerContext context, XmlNode node)
        {
            switch (foundation)
            {
                case Material material:
                    material.DefaultVSEntryPoint = node.InnerText;
                    return new NodeParseResult(NodeParseResultType.Success);
                case MaterialPass pass:
                    pass.VertexShader.EntryPoint = node.InnerText;
                    return new NodeParseResult(NodeParseResultType.Success);
            }

            return new NodeParseResult(NodeParseResultType.Ignored);
        }
    }
}
