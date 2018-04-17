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
        public ShaderPassParser(string nodeName) : base(nodeName) { }

        internal override NodeParseResult Parse(HlslFoundation shader, ShaderCompilerContext context, XmlNode node)
        {
            if (shader is Material material)
            {
                MaterialPass pass = new MaterialPass(material)
                {
                    Iterations = 1,
                };

                foreach (XmlNode child in node.ChildNodes)
                {
                    string nodeName = child.Name.ToLower();
                    switch (nodeName)
                    {
                        case "name":
                            pass.Name = child.InnerText;
                            break;

                        case "iterations":
                            int val = 1;
                            if (!int.TryParse(child.InnerText, out val))
                            {
                                // TODO spit out a warning stating the iterations data format is invalid
                            }
                            else
                            {
                                pass.Iterations = val;
                            }
                            break;

                        case "vertex":
                            pass.VertexShader.EntryPoint = child.InnerText;
                            break;

                        case "geometry":
                            pass.GeometryShader.EntryPoint = child.InnerText;
                            break;

                        case "hull":
                            pass.HullShader.EntryPoint = child.InnerText;
                            break;

                        case "domain":
                            pass.DomainShader.EntryPoint = child.InnerText;
                            break;

                        case "pixel":
                            pass.PixelShader.EntryPoint = child.InnerText;
                            break;

                            //case "rasterizer":
                            //    //pass.RasterizerState = context.Compiler.RasterizerParser.Parse(shader, context, child);
                            //    break;
                    }
                }

                if (string.IsNullOrWhiteSpace(pass.VertexShader.EntryPoint) || string.IsNullOrWhiteSpace(pass.PixelShader.EntryPoint))
                    return new NodeParseResult(NodeParseResultType.Error, "A material pass must have at least a vertex and pixel entry point.");

                // Add the pass once for each iteration it is meant to run.
                for (int i = 0; i < pass.Iterations; i++)
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
