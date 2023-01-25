using Silk.NET.Direct3D11;
using System.Xml;

namespace Molten.Graphics
{
    internal class DepthNodeParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Depth;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            DepthStateDX11 state = null;
            DepthStencilPreset preset = DepthStencilPreset.Default;

            if (node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (!Enum.TryParse(node.Value, true, out preset))
                    InvalidEnumMessage<DepthStencilPreset>(context, (node.Name, node.Value), "depth-stencil preset");
            }

            state = new DepthStateDX11(foundation.Device as DeviceDX11, foundation.Device.DepthBank.GetPreset(preset) as DepthStateDX11);

            foreach ((string Name, string Value) c in node.ChildValues)
            {
                switch (c.Name)
                {
                    case "writepermission":
                        if (EngineUtil.TryParseEnum(c.Value, out GraphicsDepthWritePermission writePermission))
                            state.WritePermission = writePermission;
                        else
                            InvalidEnumMessage<DepthWriteMask>(context, c, "depth write permission");
                        break;

                    case "enabled":
                        if (bool.TryParse(c.Value, out bool depthEnabled))
                            state.IsDepthEnabled = depthEnabled;
                        else
                            InvalidValueMessage(context, c, "depth-testing enabled", "boolean");
                        break;

                    case "stencilenabled":
                        if (bool.TryParse(c.Value, out bool stencilEnabled))
                            state.IsStencilEnabled = stencilEnabled;
                        else
                            InvalidValueMessage(context, c, "stencil-testing enabled", "boolean");
                        break;

                    case "writemask":
                        if (EngineUtil.TryParseEnum(c.Value, out DepthWriteFlags writeFlags))
                            state.WriteFlags = writeFlags;
                        else
                            InvalidEnumMessage<DepthWriteMask>(context, c, "depth write mask");
                        break;

                    case "comparison":
                        if (EngineUtil.TryParseEnum(c.Value, out ComparisonFunction comparison))
                            state.DepthComparison = comparison;
                        else
                            InvalidEnumMessage<ComparisonFunc>(context, c, "depth comparison");
                        break;

                    case "stencilreadmask":
                        if (byte.TryParse(c.Value, out byte stencilReadMask))
                            state.StencilReadMask = stencilReadMask;
                        else
                            InvalidValueMessage(context, c, "stencil read mask", "byte");
                        break;

                    case "stencilwritemask":
                        if (byte.TryParse(c.Value, out byte stencilWriteMask))
                            state.StencilWriteMask = stencilWriteMask;
                        else
                            InvalidValueMessage(context, c, "stencil write mask", "byte");
                        break;

                    case "reference":
                        if (byte.TryParse(c.Value, out byte stencilRef))
                            state.StencilReference = stencilRef;
                        else
                            InvalidValueMessage(context, c, "stencil reference", "integer");
                        break;

                    default:
                        UnsupportedTagMessage(context, node.Name, c);
                        break;
                }
            }

            foreach (ShaderHeaderNode c in node.ChildNodes)
            {
                switch (c.Name)
                {
                    case "front":
                        ParseFaceNode(context, c, state.FrontFace);
                        break;

                    case "back":
                        ParseFaceNode(context, c, state.BackFace);
                        break;
                }
            }

            state = foundation.Device.DepthBank.AddOrRetrieveExisting(state) as DepthStateDX11;

            if (node.Conditions == StateConditions.None)
                foundation.DepthState.FillMissingWith(state);
            else
                foundation.DepthState[node.Conditions] = state;
        }

        private void ParseFaceNode(ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode faceNode, GraphicsDepthState.Face face)
        {
            foreach (ShaderHeaderNode c in faceNode.ChildNodes)
            {
                (string Name, string Value) cv = (c.Name, c.Value);

                switch (c.Name) {
                    case "comparison":
                        if (EngineUtil.TryParseEnum(c.Value, out ComparisonFunction comparison))
                            face.Comparison = comparison;
                        else
                            InvalidEnumMessage<ComparisonFunction>(context, cv, $"{faceNode.Name}-face comparison");
                        break;

                    case "stencilpass":
                        if (EngineUtil.TryParseEnum(c.Value, out DepthStencilOperation stencilpassOp))
                            face.PassOperation = stencilpassOp;
                        else
                            InvalidEnumMessage<DepthStencilOperation>(context, cv, $"{faceNode.Name}-face stencil pass operation");
                        break;

                    case "stencilfail":
                        if (EngineUtil.TryParseEnum(c.Value, out DepthStencilOperation stencilFailOp))
                            face.FailOperation = stencilFailOp;
                        else
                            InvalidEnumMessage<DepthStencilOperation>(context, cv, $"{faceNode.Name}-face stencil fail operation");
                        break;

                    case "fail":
                        if (EngineUtil.TryParseEnum(c.Value, out DepthStencilOperation failOp))
                            face.DepthFailOperation = failOp;
                        else
                            InvalidEnumMessage<DepthStencilOperation>(context, cv, $"{faceNode.Name}-face depth fail operation");
                        break;

                    default:
                        UnsupportedTagMessage(context, faceNode.Name, cv);
                        break;
                }
            }
        }
    }
}
