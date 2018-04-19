using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class BlendNodeParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "blend" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
                return new NodeParseResult(NodeParseResultType.Ignored);

            // Check if an existing state was already set.
            GraphicsBlendState state = null;
            switch (foundation)
            {
                case Material material:
                    state = material.BlendState;
                    break;

                case MaterialPass pass:
                    state = pass.BlendState;
                    break;
            }

            int rtIndex = 0;
            bool existingState = state != null;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out BlendPreset preset))
                            state = new GraphicsBlendState(foundation.Device.GetPreset(preset));
                        break;

                    case "index": // The blend state RT index. Blend states can provide per-render target/surface configuration.
                        int.TryParse(attribute.InnerText, out rtIndex);
                        break;                            
                }
            }

            state = state ?? new GraphicsBlendState(foundation.Device.GetPreset(BlendPreset.Default));
            state.IndependentBlendEnable = state.IndependentBlendEnable || (rtIndex > 0);

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "enabled":
                        if (bool.TryParse(child.InnerText, out bool blendEnabled))
                            state.SetIsBlendEnabled(rtIndex, blendEnabled);
                        else
                            InvalidValueMessage(context, child, "blend enabled", "boolean");
                        break;

                    case "alphatocoverage":
                        if (bool.TryParse(child.InnerText, out bool alphaToCoverage))
                            state.AlphaToCoverageEnable = alphaToCoverage;
                        else
                            InvalidValueMessage(context, child, "alpha-to-coverage enabled", "boolean");
                        break;

                    case "source":
                        if (Enum.TryParse(child.InnerText, true, out BlendOption sourceBlend))
                            state.SetSourceBlend(rtIndex, sourceBlend);
                        else
                            InvalidEnumMessage<BlendOption>(context, child, "source blend option");
                        break;

                    case "destination":
                        if (Enum.TryParse(child.InnerText, true, out BlendOption destBlend))
                            state.SetDestinationBlend(rtIndex, destBlend);
                        else
                            InvalidEnumMessage<BlendOption>(context, child, "destination blend option");
                        break;

                    case "operation":
                        if (Enum.TryParse(child.InnerText, true, out BlendOperation blendOp))
                            state.SetBlendOperation(rtIndex, blendOp);
                        else
                            InvalidEnumMessage<BlendOperation>(context, child, "blend operation");
                        break;

                    case "sourcealpha":
                        if (Enum.TryParse(child.InnerText, true, out BlendOption sourceAlpha))
                            state.SetSourceAlphaBlend(rtIndex, sourceAlpha);
                        else
                            InvalidEnumMessage<BlendOption>(context, child, "source alpha option");
                        break;

                    case "destinationalpha":
                        if (Enum.TryParse(child.InnerText, true, out BlendOption destAlpha))
                            state.SetDestinationAlphaBlend(rtIndex, destAlpha);
                        else
                            InvalidEnumMessage<BlendOption>(context, child, "destination alpha option");
                        break;

                    case "alphaoperation":
                        if (Enum.TryParse(child.InnerText, true, out BlendOperation alphaOperation))
                            state.SetAlphaBlendOperation(rtIndex, alphaOperation);
                        else
                            InvalidEnumMessage<BlendOperation>(context, child, "alpha-blend operation");
                        break;

                    case "writemask":
                        if (Enum.TryParse(child.InnerText, true, out ColorWriteMaskFlags rtWriteMask))
                            state.SetRenderTargetWriteMask(rtIndex, rtWriteMask);
                        else
                            InvalidEnumMessage<ColorWriteMaskFlags>(context, child, "render surface/target write mask");
                        break;

                    default:
                        UnsupportedTagMessage(context, child);
                        break;
                }
            }

            // Check if an identical state exists (if we don't already have one) before returning the new one.
            if (!existingState)
            {
                foreach (GraphicsBlendState existing in context.BlendStates)
                {
                    if (existing.Equals(state))
                    {
                        state.Dispose();
                        existingState = true;
                        state = existing;
                        break;
                    }
                }
            }

            // If the defined state still isn't an existing one, add it to the context.
            if (!existingState)
                context.BlendStates.Add(state);

            switch (foundation)
            {
                case Material material:
                    material.BlendState = state;

                    // Apply to existing passes which do not have a rasterizer state yet.
                    foreach (MaterialPass p in material.Passes)
                    {
                        if (p.BlendState == null)
                            p.BlendState = state;
                    }
                    break;

                case MaterialPass pass:
                    pass.BlendState = state;
                    break;
            }

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
