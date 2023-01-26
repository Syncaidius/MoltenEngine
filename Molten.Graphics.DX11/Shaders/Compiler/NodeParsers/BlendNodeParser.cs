using Silk.NET.Direct3D11;
using System.Xml;

namespace Molten.Graphics
{
    public class BlendNodeParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Blend;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            GraphicsBlendState template = foundation.Device.BlendBank.NewFromPreset(BlendPreset.Default) as BlendStateDX11;
            GraphicsBlendState.RenderSurfaceBlend rtBlend = template[0]; // Use the default preset's first (0) RT blend description.

            if(node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (Enum.TryParse(node.Value, true, out BlendPreset preset))
                {
                    // Use a template preset's first (0) RT blend description.
                    template = foundation.Device.BlendBank.GetPreset(preset) as BlendStateDX11;
                    rtBlend = template[0];
                }
                else
                {
                    InvalidEnumMessage<BlendPreset>(context, (node.Name, node.Value), "blend preset");
                }
            }

            GraphicsBlendState state = foundation.Device.CreateBlendState(foundation.BlendState[node.Conditions]) ?? foundation.Device.BlendBank.GetPreset(BlendPreset.Default);
            state.IndependentBlendEnable = (state.IndependentBlendEnable || (node.SlotID > 0));

            foreach ((string Name, string Value) c in node.ChildValues)
            {
                switch (c.Name)
                {
                    case "enabled":
                        if (bool.TryParse(c.Value, out bool blendEnabled))
                            rtBlend.BlendEnable = blendEnabled ? 1 : 0;
                        else
                            InvalidValueMessage(context, c, "blend enabled", "boolean");
                        break;

                    case "alphatocoverage":
                        if (bool.TryParse(c.Value, out bool alphaToCoverage))
                            state.AlphaToCoverageEnable = alphaToCoverage;
                        else
                            InvalidValueMessage(context, c, "alpha-to-coverage enabled", "boolean");
                        break;

                    case "source":
                        if (Enum.TryParse(c.Value, true, out BlendType sourceBlend))
                            rtBlend.SrcBlend = sourceBlend;
                        else
                            InvalidEnumMessage<BlendType>(context, c, "source blend option");
                        break;

                    case "destination":
                        if (Enum.TryParse(c.Value, true, out BlendType destBlend))
                            rtBlend.DestBlend = destBlend;
                        else
                            InvalidEnumMessage<BlendType>(context, c, "destination blend option");
                        break;

                    case "operation":
                        if (Enum.TryParse(c.Value, true, out BlendOperation blendOp))
                            rtBlend.BlendOp = blendOp;
                        else
                            InvalidEnumMessage<BlendOperation>(context, c, "blend operation");
                        break;

                    case "sourcealpha":
                        if (Enum.TryParse(c.Value, true, out BlendType sourceAlpha))
                            rtBlend.SrcBlendAlpha = sourceAlpha;
                        else
                            InvalidEnumMessage<BlendType>(context, c, "source alpha option");
                        break;

                    case "destinationalpha":
                        if (Enum.TryParse(c.Value, true, out BlendType destAlpha))
                            rtBlend.DestBlendAlpha = destAlpha;
                        else
                            InvalidEnumMessage<BlendType>(context, c, "destination alpha option");
                        break;

                    case "alphaoperation":
                        if (Enum.TryParse(c.Value, true, out BlendOperation alphaOperation))
                            rtBlend.BlendOpAlpha = alphaOperation;
                        else
                            InvalidEnumMessage<BlendOperation>(context, c, "alpha-blend operation");
                        break;

                    case "writemask":
                        if (Enum.TryParse(c.Value, true, out ColorWriteFlags rtWriteMask))
                            rtBlend.RenderTargetWriteMask = rtWriteMask;
                        else
                            InvalidEnumMessage<ColorWriteFlags>(context, c, "render surface/target write mask");
                        break;

                    case "samplemask":
                        if (uint.TryParse(c.Value, out uint mask))
                            state.BlendSampleMask = mask;
                        else
                            InvalidValueMessage(context, c, "sample mask", "unsigned integer");
                        break;

                    case "factor":
                        state.BlendFactor = ParseColor4(context, c.Value, false);
                        break;
                }
            }

            // Update RT blend description on main description.
            state[node.SlotID].Set(rtBlend);
            state = foundation.Device.BlendBank.AddOrRetrieveExisting(state);

            if (node.Conditions == StateConditions.None)
                foundation.BlendState.FillMissingWith(state as BlendStateDX11);
            else
                foundation.BlendState[node.Conditions] = state as BlendStateDX11;
        }
    }
}
