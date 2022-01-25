using Silk.NET.Direct3D11;
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

        internal override NodeParseResult Parse(HlslFoundation foundation, HlslCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
                return new NodeParseResult(NodeParseResultType.Ignored);

            StateConditions conditions = StateConditions.None;
            int rtIndex = 0;

            GraphicsBlendState template = foundation.Device.BlendBank.GetPreset(BlendPreset.Default);
            RenderTargetBlendDesc1 rtBlendDesc = template.GetSurfaceBlendState(0); // Use the default preset's first (0) RT blend description.

            // Prerequisit attributes
            foreach (XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "condition":
                        if (!Enum.TryParse(attribute.InnerText, true, out conditions))
                            InvalidEnumMessage<StateConditions>(context, attribute, "state condition");
                        break;

                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out BlendPreset preset))
                        {
                            // Use a template preset's first (0) RT blend description.
                            template = foundation.Device.BlendBank.GetPreset(preset);
                            rtBlendDesc = template.GetSurfaceBlendState(0);
                        }
                        break;

                    case "index": // The blend state RT index. Blend states can provide per-render target/surface configuration.
                        int.TryParse(attribute.InnerText, out rtIndex);
                        break;
                }
            }

            // Use existing state if present, or create a new one.
            GraphicsBlendState state = new GraphicsBlendState(foundation.Device, foundation.BlendState[conditions] ?? foundation.Device.BlendBank.GetPreset(BlendPreset.Default));
            state.IndependentBlendEnable = (state.IndependentBlendEnable || (rtIndex > 0));

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "enabled":
                        if (bool.TryParse(child.InnerText, out bool blendEnabled))
                            rtBlendDesc.BlendEnable = blendEnabled ? 1 : 0;
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
                        if (Enum.TryParse(child.InnerText, true, out Blend sourceBlend))
                            rtBlendDesc.SrcBlend = sourceBlend;
                        else
                            InvalidEnumMessage<Blend>(context, child, "source blend option");
                        break;

                    case "destination":
                        if (Enum.TryParse(child.InnerText, true, out Blend destBlend))
                            rtBlendDesc.DestBlend = destBlend;
                        else
                            InvalidEnumMessage<Blend>(context, child, "destination blend option");
                        break;

                    case "operation":
                        if (Enum.TryParse(child.InnerText, true, out BlendOp blendOp))
                            rtBlendDesc.BlendOp = blendOp;
                        else
                            InvalidEnumMessage<BlendOp>(context, child, "blend operation");
                        break;

                    case "sourcealpha":
                        if (Enum.TryParse(child.InnerText, true, out Blend sourceAlpha))
                            rtBlendDesc.SrcBlendAlpha = sourceAlpha;
                        else
                            InvalidEnumMessage<Blend>(context, child, "source alpha option");
                        break;

                    case "destinationalpha":
                        if (Enum.TryParse(child.InnerText, true, out Blend destAlpha))
                            rtBlendDesc.DestBlendAlpha = destAlpha;
                        else
                            InvalidEnumMessage<Blend>(context, child, "destination alpha option");
                        break;

                    case "alphaoperation":
                        if (Enum.TryParse(child.InnerText, true, out BlendOp alphaOperation))
                            rtBlendDesc.BlendOpAlpha = alphaOperation;
                        else
                            InvalidEnumMessage<BlendOp>(context, child, "alpha-blend operation");
                        break;

                    case "writemask":
                        if (Enum.TryParse(child.InnerText, true, out ColorWriteEnable rtWriteMask))
                            rtBlendDesc.RenderTargetWriteMask = (byte)rtWriteMask;
                        else
                            InvalidEnumMessage<ColorWriteEnable>(context, child, "render surface/target write mask");
                        break;

                    case "samplemask":
                        if (uint.TryParse(child.InnerText, out uint mask))
                            state.BlendSampleMask = mask;
                        else
                            InvalidValueMessage(context, child, "sample mask", "unsigned integer");
                        break;

                    case "factor":
                        state.BlendFactor = ParseColor4(context, node, false);
                        break;

                    default:
                        UnsupportedTagMessage(context, child);
                        break;
                }
            }

            // Update RT blend description on main description.
            state[rtIndex] = rtBlendDesc;
            state = foundation.Device.BlendBank.AddOrRetrieveExisting(state);

            if (conditions == StateConditions.None)
                foundation.BlendState.FillMissingWith(state);
            else
                foundation.BlendState[conditions] = state;

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
