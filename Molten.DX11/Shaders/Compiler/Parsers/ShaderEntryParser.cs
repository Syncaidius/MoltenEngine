﻿using System;
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
        internal override string[] SupportedNodes => new string[] { "entry" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
            {
                ComputeTask cTask = foundation as ComputeTask;
                if (string.IsNullOrWhiteSpace(node.InnerText))
                    return new NodeParseResult(NodeParseResultType.Error, "Compute task <entry> tag is missing or empty.");
                else
                    cTask.Composition.EntryPoint = node.InnerText;

                return new NodeParseResult(NodeParseResultType.Success);
            }

            return new NodeParseResult(NodeParseResultType.Ignored);
        }
    }
}
