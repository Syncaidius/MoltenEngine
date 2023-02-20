namespace Molten.Graphics
{
    internal class StateNodeParser : ShaderNodeParser
    {
        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        public override ShaderNodeType NodeType => ShaderNodeType.State;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            GraphicsPipelineState state = foundation.Device.CreateState(PipelineStatePreset.Default);
            GraphicsPipelineState.RenderSurfaceBlend rtBlend = state[0]; // Use the default preset's first (0) RT blend description.
            PipelineStatePreset preset = PipelineStatePreset.Default;

            switch (node.ValueType)
            {
                case ShaderHeaderValueType.Preset:
                    {
                        if (Enum.TryParse(node.Value, true, out preset))
                        {
                            if (preset != PipelineStatePreset.Default)
                                foundation.Device.StatePresets.ApplyPreset(state, preset);

                            rtBlend = state[0];
                        }
                        else
                        {
                            InvalidEnumMessage<PipelineStatePreset>(context, (node.Name, node.Value), "pipeline preset");
                        }
                    }
                    break;

                case ShaderHeaderValueType.BlendPreset:
                    {
                        if (Enum.TryParse(node.Value, true, out BlendPreset subPreset))
                        {
                            if (subPreset != BlendPreset.Default)
                                foundation.Device.StatePresets.ApplyBlendPreset(state, subPreset);
                        }
                        else
                        {
                            InvalidEnumMessage<BlendPreset>(context, (node.Name, node.Value), "blend preset");
                        }
                    }
                    break;

                case ShaderHeaderValueType.RasterizerPreset:
                    {
                        if (Enum.TryParse(node.Value, true, out RasterizerPreset subPreset))
                        {
                            if (subPreset != RasterizerPreset.Default)
                                foundation.Device.StatePresets.ApplyRasterizerPreset(state, subPreset);
                        }
                        else
                        {
                            InvalidEnumMessage<RasterizerPreset>(context, (node.Name, node.Value), "rasterizer preset");
                        }
                    }
                    break;

                case ShaderHeaderValueType.DepthPreset:
                    {
                        if (Enum.TryParse(node.Value, true, out DepthStencilPreset subPreset))
                        {
                            if (subPreset != DepthStencilPreset.Default)
                                foundation.Device.StatePresets.ApplyDepthPreset(state, subPreset);
                        }
                        else
                        {
                            InvalidEnumMessage<DepthStencilPreset>(context, (node.Name, node.Value), "depth-stencil preset");
                        }
                    }
                    break;
            }

            state = foundation.State[node.Conditions] ?? state;
            ParseProperties(node, context, state);

            state.IndependentBlendEnable = (state.IndependentBlendEnable || (node.SlotID > 0));

            // Update RT blend description on main description.
            state[node.SlotID].Set(rtBlend);

            if (node.Conditions == StateConditions.None)
                foundation.State.FillMissingWith(state);
            else
                foundation.State[node.Conditions] = state;
        }
    }
}
