using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class RasterizerNodeParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "rasterizer" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
                return new NodeParseResult(NodeParseResultType.Ignored);

            GraphicsRasterizerState state = new GraphicsRasterizerState(foundation.Device.GetPreset(RasterizerPreset.Default));

            foreach(XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out RasterizerPreset preset))
                            state = new GraphicsRasterizerState(foundation.Device.GetPreset(preset));
                        break;
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "cullmode":
                        if (Enum.TryParse(child.InnerText, true, out CullMode mode))
                            state.CullMode = mode;
                        break;

                    case "depthbias":
                        if (int.TryParse(child.InnerText, out int bias))
                            state.DepthBias = bias;
                        break;

                    case "depthbiasclamp":
                        if (float.TryParse(child.InnerText, out float biasClamp))
                            state.DepthBiasClamp = biasClamp;
                        break;

                    case "fillmode":
                        if (Enum.TryParse(child.InnerText, true, out FillMode fillMode))
                            state.FillMode = fillMode;
                        break;

                    case "aaline": // IsAntialiasedLineEnabled
                        if (bool.TryParse(child.InnerText, out bool aaLineEnabled))
                            state.IsAntialiasedLineEnabled = aaLineEnabled;
                        break;

                    case "depthclip":
                        if (bool.TryParse(child.InnerText, out bool depthClipEnabled))
                            state.IsDepthClipEnabled = depthClipEnabled;
                        break;

                    case "frontisccw":
                        if (bool.TryParse(child.InnerText, out bool frontIsCCW))
                            state.IsFrontCounterClockwise = frontIsCCW;
                        break;

                    case "multisample":
                        if (bool.TryParse(child.InnerText, out bool multisampleEnabled))
                            state.IsMultisampleEnabled = multisampleEnabled;
                        break;

                    case "scissortest":
                        if (bool.TryParse(child.InnerText, out bool scissorEnabled))
                            state.IsScissorEnabled = scissorEnabled;
                        break;

                    case "scaledslopebias":
                        if (float.TryParse(child.InnerText, out float scaledSlopeDepthBias))
                            state.SlopeScaledDepthBias = scaledSlopeDepthBias;
                        break;
                }
            }

            bool existingState = false;
            // Check if an identical state exists before returning the new one.
            foreach (GraphicsRasterizerState existing in context.RasterStates)
            {
                if (existing.Equals(state))
                {
                    state.Dispose();
                    existingState = true;
                    state = existing;
                    break;
                }
            }

            if (!existingState)
                context.RasterStates.Add(state);

            switch (foundation)
            {
                case Material material:
                    material.RasterizerState = state;

                    // Apply to existing passes which do not have a rasterizer state yet.
                    foreach(MaterialPass p in material.Passes)
                    {
                        if (p.RasterizerState == null)
                            p.RasterizerState = state;
                    }
                    break;

                case MaterialPass pass:
                    pass.RasterizerState = state;
                    break;
            }

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
