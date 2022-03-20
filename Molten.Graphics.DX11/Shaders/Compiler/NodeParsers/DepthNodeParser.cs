using Silk.NET.Direct3D11;
using System.Xml;

namespace Molten.Graphics
{
    internal class DepthNodeParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Depth;

        public override void Parse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, XmlNode node)
        {
            if (foundation is ComputeTask)
            {
                context.AddWarning($"Ignoring {NodeType} in compute task definition");
                return;
            }

            GraphicsDepthState state = null;
            StateConditions conditions = StateConditions.None;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                string attName = attribute.Name.ToLower();
                switch (attName)
                {
                    case "preset":
                        if (Enum.TryParse(attribute.InnerText, true, out DepthStencilPreset preset))
                            state = new GraphicsDepthState(foundation.Device, foundation.Device.DepthBank.GetPreset(preset));
                        break;

                    case "condition":
                        if (!Enum.TryParse(attribute.InnerText, true, out conditions))
                            InvalidEnumMessage<StateConditions>(context, attribute, "state condition");
                        break;
                }
            }

            state = state ?? new GraphicsDepthState(foundation.Device, foundation.Device.DepthBank.GetPreset(DepthStencilPreset.Default));

            foreach (XmlNode child in node.ChildNodes)
            {
                string nodeName = child.Name.ToLower();

                switch (nodeName)
                {
                    case "writepermission":
                        if (SilkUtil.TryParseEnum(child.InnerText, out GraphicsDepthWritePermission writePermission))
                            state.WritePermission = writePermission;
                        else
                            InvalidEnumMessage<DepthWriteMask>(context, child, "depth write permission");
                        break;

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
                        if (SilkUtil.TryParseEnum(child.InnerText, out DepthWriteMask writeMask))
                            state.DepthWriteMask = writeMask;
                        else
                            InvalidEnumMessage<DepthWriteMask>(context, child, "depth write mask");
                        break;

                    case "comparison":
                        if (SilkUtil.TryParseEnum(child.InnerText, out ComparisonFunc comparison))
                            state.DepthComparison = comparison;
                        else
                            InvalidEnumMessage<ComparisonFunc>(context, child, "depth comparison");
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

                    case "reference":
                        if (byte.TryParse(child.InnerText, out byte stencilRef))
                            state.StencilReference = stencilRef;
                        else
                            InvalidValueMessage(context, child, "stencil reference", "integer");
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

            state = foundation.Device.DepthBank.AddOrRetrieveExisting(state);

            if (conditions == StateConditions.None)
                foundation.DepthState.FillMissingWith(state);
            else
                foundation.DepthState[conditions] = state;
        }

        private void ParseFaceNode(ShaderCompilerContext<RendererDX11, HlslFoundation> context, XmlNode faceNode, GraphicsDepthState.Face face)
        {
            foreach(XmlNode child in faceNode.ChildNodes)
            {
                string nodeName = child.Name.ToLower();
                switch (nodeName) {
                    case "comparison":
                        if (SilkUtil.TryParseEnum(child.InnerText, out ComparisonFunc comparison))
                            face.Comparison = comparison;
                        else
                            InvalidEnumMessage<ComparisonFunc>(context, child, $"{faceNode.Name}-face comparison");
                        break;

                    case "stencilpass":
                        if (SilkUtil.TryParseEnum(child.InnerText, out StencilOp stencilpassOp))
                            face.PassOperation = stencilpassOp;
                        else
                            InvalidEnumMessage<StencilOp>(context, child, $"{faceNode.Name}-face stencil pass operation");
                        break;

                    case "stencilfail":
                        if (SilkUtil.TryParseEnum(child.InnerText, out StencilOp stencilFailOp))
                            face.FailOperation = stencilFailOp;
                        else
                            InvalidEnumMessage<StencilOp>(context, child, $"{faceNode.Name}-face stencil fail operation");
                        break;

                    case "fail":
                        if (SilkUtil.TryParseEnum(child.InnerText, out StencilOp failOp))
                            face.DepthFailOperation = failOp;
                        else
                            InvalidEnumMessage<StencilOp>(context, child, $"{faceNode.Name}-face depth fail operation");
                        break;

                    default:
                        UnsupportedTagMessage(context, child);
                        break;
                }
            }
        }
    }
}
