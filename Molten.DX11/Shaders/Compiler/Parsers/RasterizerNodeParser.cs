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

            GraphicsRasterizerState state = null;
            StateConditions conditions = StateConditions.None;

            foreach(XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out RasterizerPreset preset))
                            state = new GraphicsRasterizerState(foundation.Device.RasterizerBank.GetPreset(preset));
                        break;

                    case "condition":
                        if (!Enum.TryParse(attribute.InnerText, true, out conditions))
                            InvalidEnumMessage<StateConditions>(context, attribute, "state condition");
                        break;
                }
            }

            state = state ?? new GraphicsRasterizerState(foundation.Device.RasterizerBank.GetPreset(RasterizerPreset.Default));

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "cullmode":
                        if (Enum.TryParse(child.InnerText, true, out CullMode mode))
                            state.CullMode = mode;
                        else
                            InvalidEnumMessage<CullMode>(context, child, "cull mode");
                        break;

                    case "depthbias":
                        if (int.TryParse(child.InnerText, out int bias))
                            state.DepthBias = bias;
                        else
                            InvalidValueMessage(context, child, "depth bias", "integer");
                        break;

                    case "depthbiasclamp":
                        if (float.TryParse(child.InnerText, out float biasClamp))
                            state.DepthBiasClamp = biasClamp;
                        else
                            InvalidValueMessage(context, child, "depth bias clamp", "floating-point");
                        break;

                    case "fill":
                        if (Enum.TryParse(child.InnerText, true, out FillMode fillMode))
                            state.FillMode = fillMode;
                        else
                            InvalidEnumMessage<FillMode>(context, child, "fill mode");
                        break;

                    case "aaline": // IsAntialiasedLineEnabled
                        if (bool.TryParse(child.InnerText, out bool aaLineEnabled))
                            state.IsAntialiasedLineEnabled = aaLineEnabled;
                        break;

                    case "depthclip":
                        if (bool.TryParse(child.InnerText, out bool depthClipEnabled))
                            state.IsDepthClipEnabled = depthClipEnabled;
                        else
                            InvalidValueMessage(context, child, "depth clip enabled", "boolean");
                        break;

                    case "frontisccw":
                        if (bool.TryParse(child.InnerText, out bool frontIsCCW))
                            state.IsFrontCounterClockwise = frontIsCCW;
                        else
                            InvalidValueMessage(context, child, "front is counter-clockwise", "boolean");
                        break;

                    case "multisample":
                        if (bool.TryParse(child.InnerText, out bool multisampleEnabled))
                            state.IsMultisampleEnabled = multisampleEnabled;
                        else
                            InvalidValueMessage(context, child, "multisampling enabled", "boolean");
                        break;

                    case "scissortest":
                        if (bool.TryParse(child.InnerText, out bool scissorEnabled))
                            state.IsScissorEnabled = scissorEnabled;
                        else
                            InvalidValueMessage(context, child, "scissor testing enabled", "boolean");
                        break;

                    case "scaledslopebias":
                        if (float.TryParse(child.InnerText, out float scaledSlopeDepthBias))
                            state.SlopeScaledDepthBias = scaledSlopeDepthBias;
                        else
                            InvalidValueMessage(context, child, "slope-scaled depth bias", "floating-point");
                        break;

                    default:
                        UnsupportedTagMessage(context, child);
                        break;
                }
            }
            state = foundation.Device.RasterizerBank.AddOrRetrieveExisting(state);

            if (conditions == StateConditions.None)
                foundation.RasterizerState.FillMissingWith(state);
            else
                foundation.RasterizerState[conditions] = state;

            return new NodeParseResult(NodeParseResultType.Success);
        }
    }
}
