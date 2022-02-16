using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ComputeEntryParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Entry;

        public override void Parse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, XmlNode node)
        {
            if (foundation is ComputeTask cTask)
            {
                if (string.IsNullOrWhiteSpace(node.InnerText))
                    context.AddError("Compute task <entry> tag is missing or empty.");
                else
                    cTask.Composition.EntryPoint = node.InnerText;
            }
            else
            {
                context.AddWarning($"Ignoring '{NodeType}' for unsupported shader type '{foundation.GetType().Name}'");
            }
        }
    }
}
