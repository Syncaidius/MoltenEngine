using Silk.NET.Direct3D11;
using System.Xml;

namespace Molten.Graphics
{
    public class BlendNodeParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Blend;

        public override Type[] TypeFilter { get; } = { typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            StateConditions conditions = StateConditions.None;

            GraphicsBlendState template = foundation.Device.BlendBank.GetPreset(BlendPreset.Default);
            RenderTargetBlendDesc rtBlendDesc = template.GetSurfaceBlendState(0); // Use the default preset's first (0) RT blend description.

            // Prerequisit attributes
            foreach ((string Name, string Value) c in node.ChildValues)
            {
                switch (c.Name)
                {
                    case "condition":
                        if (!Enum.TryParse(c.Value, true, out conditions))
                            InvalidEnumMessage<StateConditions>(context, c, "state condition");
                        break;

                    case "preset":
                        if (Enum.TryParse(c.Value, true, out BlendPreset preset))
                        {
                            // Use a template preset's first (0) RT blend description.
                            template = foundation.Device.BlendBank.GetPreset(preset);
                            rtBlendDesc = template.GetSurfaceBlendState(0);
                        }
                        break;
                }
            }

            GraphicsBlendState state = new GraphicsBlendState(foundation.Device, foundation.BlendState[conditions] ?? foundation.Device.BlendBank.GetPreset(BlendPreset.Default));
            state.IndependentBlendEnable = (state.IndependentBlendEnable || (node.Index > 0));

            foreach ((string Name, string Value) c in node.ChildValues)
            {
                switch (c.Name)
                {
                    case "enabled":
                        if (bool.TryParse(c.Value, out bool blendEnabled))
                            rtBlendDesc.BlendEnable = blendEnabled ? 1 : 0;
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
                        if (Enum.TryParse(c.Value, true, out Blend sourceBlend))
                            rtBlendDesc.SrcBlend = sourceBlend;
                        else
                            InvalidEnumMessage<Blend>(context, c, "source blend option");
                        break;

                    case "destination":
                        if (Enum.TryParse(c.Value, true, out Blend destBlend))
                            rtBlendDesc.DestBlend = destBlend;
                        else
                            InvalidEnumMessage<Blend>(context, c, "destination blend option");
                        break;

                    case "operation":
                        if (Enum.TryParse(c.Value, true, out BlendOp blendOp))
                            rtBlendDesc.BlendOp = blendOp;
                        else
                            InvalidEnumMessage<BlendOp>(context, c, "blend operation");
                        break;

                    case "sourcealpha":
                        if (Enum.TryParse(c.Value, true, out Blend sourceAlpha))
                            rtBlendDesc.SrcBlendAlpha = sourceAlpha;
                        else
                            InvalidEnumMessage<Blend>(context, c, "source alpha option");
                        break;

                    case "destinationalpha":
                        if (Enum.TryParse(c.Value, true, out Blend destAlpha))
                            rtBlendDesc.DestBlendAlpha = destAlpha;
                        else
                            InvalidEnumMessage<Blend>(context, c, "destination alpha option");
                        break;

                    case "alphaoperation":
                        if (Enum.TryParse(c.Value, true, out BlendOp alphaOperation))
                            rtBlendDesc.BlendOpAlpha = alphaOperation;
                        else
                            InvalidEnumMessage<BlendOp>(context, c, "alpha-blend operation");
                        break;

                    case "writemask":
                        if (Enum.TryParse(c.Value, true, out ColorWriteEnable rtWriteMask))
                            rtBlendDesc.RenderTargetWriteMask = (byte)rtWriteMask;
                        else
                            InvalidEnumMessage<ColorWriteEnable>(context, c, "render surface/target write mask");
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
            state[node.Index] = rtBlendDesc;
            state = foundation.Device.BlendBank.AddOrRetrieveExisting(state);

            if (conditions == StateConditions.None)
                foundation.BlendState.FillMissingWith(state);
            else
                foundation.BlendState[conditions] = state;
        }
    }
}
