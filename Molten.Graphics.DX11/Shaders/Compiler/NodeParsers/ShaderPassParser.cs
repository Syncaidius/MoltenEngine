using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class ShaderPassParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pass;

        public override void Parse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, XmlNode node)
        {
            if (foundation is Material material)
            {
                MaterialPass pass = new MaterialPass(material, "<Unnamed Material Pass>");

                for (int i = 0; i < material.Samplers.Length; i++)
                    pass.Samplers[i] = material.Samplers[i];

                context.Compiler.ParseNode(pass, node, context);
                material.AddPass(pass);
            }
            else
            {
                context.AddWarning($"Ignoring '{NodeType}' node on non-material shader element '{foundation.GetType().Name}'");
            }
        }
    }
}
