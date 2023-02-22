namespace Molten.Graphics
{
    internal class SamplerNodeParser : StateNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Sampler;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            SamplerPreset preset = SamplerPreset.Default;

            if (node.Values.TryGetValue(ShaderHeaderValueType.Preset, out string presetValue))
            {
                if (!Enum.TryParse(presetValue, true, out preset))
                    InvalidEnumMessage<SamplerPreset>(context, (node.Name, presetValue), "sampler preset");
            }

            // Retrieve existing state if available and create a new one from it to avoid editing the existing one.
            ShaderSampler sampler = foundation.Device.CreateSampler(preset);
            ParseProperties(node, context, sampler);

            int slotID = 0;
            if (node.Values.TryGetValue(ShaderHeaderValueType.SlotID, out string slotValue))
            {
                if (!int.TryParse(slotValue, out slotID))
                    InvalidValueMessage(context, (node.Name, slotValue), "Slot ID", slotValue);
            }

            // Initialize shader state bank for the sampler if needed.
            if (slotID >= foundation.Samplers.Length)
            {
                int oldLength = foundation.Samplers.Length;
                Array.Resize(ref foundation.Samplers, slotID + 1);
                for (int i = oldLength; i < foundation.Samplers.Length; i++)
                    foundation.Samplers[i] = new ShaderStateBank<ShaderSampler>();
            }

            sampler = foundation.Device.SamplerBank.AddOrRetrieveExisting(sampler);
            foundation.Samplers[slotID][node.Conditions] = sampler;
        }
    }
}
