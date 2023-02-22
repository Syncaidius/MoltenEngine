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

            if(node.Values.TryGetValue(ShaderHeaderValueType.Preset, out string presetValue))
            {
                if (Enum.TryParse(presetValue, true, out preset))
                {
                    if (preset != PipelineStatePreset.Default)
                        foundation.Device.StatePresets.ApplyPreset(state, preset);

                    rtBlend = state[0];
                }
                else
                {
                    InvalidEnumMessage<PipelineStatePreset>(context, (node.Name, presetValue), "pipeline preset");
                }
            }

            if (node.Values.TryGetValue(ShaderHeaderValueType.BlendPreset, out string blendValue))
            {
                if (Enum.TryParse(blendValue, true, out BlendPreset subPreset))
                {
                    if (subPreset != BlendPreset.Default)
                        foundation.Device.StatePresets.ApplyBlendPreset(state, subPreset);
                }
                else
                {
                    InvalidEnumMessage<BlendPreset>(context, (node.Name, blendValue), "blend preset");
                }
            }

            if (node.Values.TryGetValue(ShaderHeaderValueType.RasterizerPreset, out string rasterValue))
            {
                if (Enum.TryParse(rasterValue, true, out RasterizerPreset subPreset))
                {
                    if (subPreset != RasterizerPreset.Default)
                        foundation.Device.StatePresets.ApplyRasterizerPreset(state, subPreset);
                }
                else
                {
                    InvalidEnumMessage<RasterizerPreset>(context, (node.Name, rasterValue), "rasterizer preset");
                }
            }

            if (node.Values.TryGetValue(ShaderHeaderValueType.DepthPreset, out string depthValue))
            {
                if (Enum.TryParse(depthValue, true, out DepthStencilPreset subPreset))
                {
                    if (subPreset != DepthStencilPreset.Default)
                        foundation.Device.StatePresets.ApplyDepthPreset(state, subPreset);
                }
                else
                {
                    InvalidEnumMessage<DepthStencilPreset>(context, (node.Name, depthValue), "depth-stencil preset");
                }
            }

            int slotID = 0;
            if(node.Values.TryGetValue(ShaderHeaderValueType.SlotID, out string slotValue))
            {
                if (!int.TryParse(slotValue, out slotID))
                    InvalidValueMessage(context, (node.Name, slotValue), "Slot ID", slotValue);
            }

            state = foundation.State[node.Conditions] ?? state;
            ParseProperties(node, context, state);

            state.IndependentBlendEnable = (state.IndependentBlendEnable || (slotID > 0));

            // Update RT blend description on main description.
            state[slotID].Set(rtBlend);

            if (node.Conditions == StateConditions.None)
                foundation.State.FillMissingWith(state);
            else
                foundation.State[node.Conditions] = state;
        }
    }
}
