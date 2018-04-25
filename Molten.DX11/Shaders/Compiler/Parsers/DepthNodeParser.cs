using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal class DepthNodeParser : ShaderNodeParser
    {
        internal override string[] SupportedNodes => new string[] { "depth" };

        internal override NodeParseResult Parse(HlslFoundation foundation, ShaderCompilerContext context, XmlNode node)
        {
            if (foundation is ComputeTask)
                return new NodeParseResult(NodeParseResultType.Ignored);

            GraphicsDepthState state = null;
            StateConditions conditions = StateConditions.None;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out DepthStencilPreset preset))
                            state = new GraphicsDepthState(foundation.Device.GetPreset(preset));
                        break;

                    case "condition":
                        if (!Enum.TryParse(attribute.InnerText, true, out conditions))
                            InvalidEnumMessage<StateConditions>(context, attribute, "state condition");
                        break;
                }
            }

            state = state ?? new GraphicsDepthState(foundation.Device.GetPreset(DepthStencilPreset.Default));

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName)
                {
                    case "enabled":
                        if (bool.TryParse(child.InnerText, out bool depthEnabled))
                            state.IsDepthEnabled = depthEnabled;
                        else
                            InvalidValueMessage(context, child, "depth-testing enabled", "boolean");
                        break;

                    case "stencilenabled":
                        if (bool.TryParse(child.InnerText, out bool stencilEnabled))
                            state.IsStencilEnabled = stencilEnabled;
                        else
                            InvalidValueMessage(context, child, "stencil-testing enabled", "boolean");
                        break;

                    case "writemask":
                        if (Enum.TryParse(child.InnerText, true, out DepthWriteMask writeMask))
                            state.DepthWriteMask = writeMask;
                        else
                            InvalidEnumMessage<DepthWriteMask>(context, child, "depth write mask");
                        break;

                    case "comparison":
                        if (Enum.TryParse(child.InnerText, true, out Comparison comparison))
                            state.DepthComparison = comparison;
                        else
                            InvalidEnumMessage<DepthWriteMask>(context, child, "depth comparison");
                        break;

                    case "stencilreadmask":
                        if (byte.TryParse(child.InnerText, out byte stencilReadMask))
                            state.StencilReadMask = stencilReadMask;
                        else
                            InvalidValueMessage(context, child, "stencil read mask", "byte");
                        break;

                    case "stencilwritemask":
                        if (byte.TryParse(child.InnerText, out byte stencilWriteMask))
                            state.StencilWriteMask = stencilWriteMask;
                        else
                            InvalidValueMessage(context, child, "stencil write mask", "byte");
                        break;

                    case "front":
                        ParseFaceNode(context, child, state.FrontFace);
                        break;

                    case "back":
                        ParseFaceNode(context, child, state.BackFace);
                        break;

                    default:
                        UnsupportedTagMessage(context, child);
                        break;
                }
            }

            bool existingState = false;
            // Check if an identical state exists before returning the new one.
            foreach (GraphicsDepthState existing in context.DepthStates)
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
                context.DepthStates.Add(state);

            foundation.DepthState[conditions] = state;
            if(foundation is Material material)
            {
                // Apply to existing passes which do not have a rasterizer state yet.
                foreach (MaterialPass p in material.Passes)
                {
                    if (p.DepthState == null)
                        p.DepthState[conditions] = state;
                }
            }

            return new NodeParseResult(NodeParseResultType.Success);
        }

        private void ParseFaceNode(ShaderCompilerContext context, XmlNode faceNode, GraphicsDepthState.Face face)
        {
            foreach(XmlNode child in faceNode.ChildNodes)
            {
                string nodeName = faceNode.Name.ToLower();
                switch (nodeName) {
                    case "comparison":
                        if (Enum.TryParse(child.InnerText, true, out Comparison comparison))
                            face.Comparison = comparison;
                        else
                            InvalidEnumMessage<Comparison>(context, child, $"{faceNode.Name}-face comparison");
                        break;

                    case "stencilpass":
                        if (Enum.TryParse(child.InnerText, true, out StencilOperation stencilpassOp))
                            face.PassOperation = stencilpassOp;
                        else
                            InvalidEnumMessage<StencilOperation>(context, child, $"{faceNode.Name}-face stencil pass operation");
                        break;

                    case "stencilfail":
                        if (Enum.TryParse(child.InnerText, true, out StencilOperation stencilFailOp))
                            face.FailOperation = stencilFailOp;
                        else
                            InvalidEnumMessage<StencilOperation>(context, child, $"{faceNode.Name}-face stencil fail operation");
                        break;

                    case "fail":
                        if (Enum.TryParse(child.InnerText, true, out StencilOperation failOp))
                            face.DepthFailOperation = failOp;
                        else
                            InvalidEnumMessage<StencilOperation>(context, child, $"{faceNode.Name}-face depth fail operation");
                        break;

                    default:
                        UnsupportedTagMessage(context, child);
                        break;
                }
            }
        }
    }
}
