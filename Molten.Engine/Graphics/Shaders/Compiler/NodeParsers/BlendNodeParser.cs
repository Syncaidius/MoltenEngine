namespace Molten.Graphics
{
    internal class BlendNodeParser : StateNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Blend;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            GraphicsBlendState template = foundation.Device.BlendBank.NewFromPreset(BlendPreset.Default);
            GraphicsBlendState.RenderSurfaceBlend rtBlend = template[0]; // Use the default preset's first (0) RT blend description.
            BlendPreset preset = BlendPreset.Default;

            if (node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (Enum.TryParse(node.Value, true, out preset))
                {
                    // Use a template preset's first (0) RT blend description.
                    template = foundation.Device.BlendBank.GetPreset(preset);
                    rtBlend = template[0];
                }
                else
                {
                    InvalidEnumMessage<BlendPreset>(context, (node.Name, node.Value), "blend preset");
                }
            }

            template = foundation.BlendState[node.Conditions] ?? foundation.Device.BlendBank.GetPreset(preset);
            GraphicsBlendState state = foundation.Device.CreateBlendState(template);
            ParseProperties(node, context, state);

            state.IndependentBlendEnable = (state.IndependentBlendEnable || (node.SlotID > 0));

            // Update RT blend description on main description.
            state[node.SlotID].Set(rtBlend);
            state = foundation.Device.BlendBank.AddOrRetrieveExisting(state);

            if (node.Conditions == StateConditions.None)
                foundation.BlendState.FillMissingWith(state);
            else
                foundation.BlendState[node.Conditions] = state;
        }
    }
}
