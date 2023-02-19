namespace Molten.Graphics
{
    internal class SamplerNodeParser : StateNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Sampler;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            SamplerPreset preset = SamplerPreset.Default;

            if (node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (!Enum.TryParse(node.Value, true, out preset))
                    InvalidEnumMessage<SamplerPreset>(context, (node.Name, node.Value), "sampler preset");
            }

            // Retrieve existing state if available and create a new one from it to avoid editing the existing one.
            ShaderSampler sampler = foundation.Device.CreateSampler(foundation.Device.SamplerBank.GetPreset(preset));
            ParseProperties(node, context, sampler);

            // Initialize shader state bank for the sampler if needed.
            if (node.SlotID >= foundation.Samplers.Length)
            {
                int oldLength = foundation.Samplers.Length;
                Array.Resize(ref foundation.Samplers, node.SlotID + 1);
                for (int i = oldLength; i < foundation.Samplers.Length; i++)
                    foundation.Samplers[i] = new ShaderStateBank<ShaderSampler>();
            }

            sampler = foundation.Device.SamplerBank.AddOrRetrieveExisting(sampler);
            foundation.Samplers[node.SlotID][node.Conditions] = sampler;
        }
    }
}
