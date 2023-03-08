namespace Molten.Graphics
{
    internal class SamplerNodeParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Sampler;

        protected override void OnParse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            GraphicsSamplerParameters parameters = new GraphicsSamplerParameters();
            SamplerPreset preset = SamplerPreset.Default;

            if (node.Values.TryGetValue(ShaderHeaderValueType.Preset, out string presetValue))
            {
                if (Enum.TryParse(presetValue, true, out preset))
                    parameters.ApplyPreset(preset);
                else
                    InvalidEnumMessage<SamplerPreset>(context, (node.Name, presetValue), "sampler preset");
            }

            ParseFields(node, context, ref parameters);

            int slotID = 0;
            if (node.Values.TryGetValue(ShaderHeaderValueType.SlotID, out string slotValue))
            {
                if (!int.TryParse(slotValue, out slotID))
                    InvalidValueMessage(context, (node.Name, slotValue), "Slot ID", slotValue);
            }

            if (slotID >= passDef.Samplers.Length)
            {
                int oldLength = passDef.Samplers.Length;
                Array.Resize(ref passDef.Samplers, slotID + 1);
                for (int i = oldLength; i < passDef.Samplers.Length; i++)
                    passDef.Samplers[i] = passDef.Samplers[0];
            }

            passDef.Samplers[slotID] = parameters;
        }
    }
}
