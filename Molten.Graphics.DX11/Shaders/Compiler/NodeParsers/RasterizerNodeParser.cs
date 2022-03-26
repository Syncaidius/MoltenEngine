using Silk.NET.Direct3D11;
using System.Xml;

namespace Molten.Graphics
{
    internal class RasterizerNodeParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Rasterizer;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            GraphicsRasterizerState state = null;
            RasterizerPreset preset = RasterizerPreset.Default;

            if (node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (!Enum.TryParse(node.Value, true, out preset))
                    InvalidEnumMessage<RasterizerPreset>(context, (node.Name, node.Value), "rasterizer preset");
            }

            state = new GraphicsRasterizerState(foundation.Device, foundation.Device.RasterizerBank.GetPreset(preset));

            foreach ((string Name, string Value) c in node.ChildValues)
            {
                switch (c.Name)
                {
                    case "cull":
                        if (EngineUtil.TryParseEnum(c.Value, out CullMode mode))
                            state.CullMode = mode;
                        else
                            InvalidEnumMessage<CullMode>(context, c, "cull mode");
                        break;

                    case "depthbias":
                        if (int.TryParse(c.Value, out int bias))
                            state.DepthBias = bias;
                        else
                            InvalidValueMessage(context, c, "depth bias", "integer");
                        break;

                    case "depthbiasclamp":
                        if (float.TryParse(c.Value, out float biasClamp))
                            state.DepthBiasClamp = biasClamp;
                        else
                            InvalidValueMessage(context, c, "depth bias clamp", "floating-point");
                        break;

                    case "fill":
                        if (EngineUtil.TryParseEnum(c.Value, out FillMode fillMode))
                            state.FillMode = fillMode;
                        else
                            InvalidEnumMessage<FillMode>(context, c, "fill mode");
                        break;

                    case "aaline": // IsAntialiasedLineEnabled
                        if (bool.TryParse(c.Value, out bool aaLineEnabled))
                            state.IsAntialiasedLineEnabled = aaLineEnabled;
                        break;

                    case "depthclip":
                        if (bool.TryParse(c.Value, out bool depthClipEnabled))
                            state.IsDepthClipEnabled = depthClipEnabled;
                        else
                            InvalidValueMessage(context, c, "depth clip enabled", "boolean");
                        break;

                    case "frontisccw":
                        if (bool.TryParse(c.Value, out bool frontIsCCW))
                            state.IsFrontCounterClockwise = frontIsCCW;
                        else
                            InvalidValueMessage(context, c, "front is counter-clockwise", "boolean");
                        break;

                    case "multisample":
                        if (bool.TryParse(c.Value, out bool multisampleEnabled))
                            state.IsMultisampleEnabled = multisampleEnabled;
                        else
                            InvalidValueMessage(context, c, "multisampling enabled", "boolean");
                        break;

                    case "scissortest":
                        if (bool.TryParse(c.Value, out bool scissorEnabled))
                            state.IsScissorEnabled = scissorEnabled;
                        else
                            InvalidValueMessage(context, c, "scissor testing enabled", "boolean");
                        break;

                    case "scaledslopebias":
                        if (float.TryParse(c.Value, out float scaledSlopeDepthBias))
                            state.SlopeScaledDepthBias = scaledSlopeDepthBias;
                        else
                            InvalidValueMessage(context, c, "slope-scaled depth bias", "floating-point");
                        break;

                    default:
                        UnsupportedTagMessage(context, node.Name, c);
                        break;
                }
            }
            state = foundation.Device.RasterizerBank.AddOrRetrieveExisting(state);

            if (node.Conditions == StateConditions.None)
                foundation.RasterizerState.FillMissingWith(state);
            else
                foundation.RasterizerState[node.Conditions] = state;
        }
    }
}
