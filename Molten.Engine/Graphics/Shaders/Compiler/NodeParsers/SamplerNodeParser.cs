namespace Molten.Graphics
{
    internal class SamplerNodeParser : StateNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Sampler;

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
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

            // Initialize shader state bank for the sampler if needed.
            // TODO This should be automatic based on the highest sampler slot read in shader reflection.
            //      All samplers should be initialized to defaults beforehand.
            if (slotID >= foundation.Samplers.Length)
            {
                int oldLength = foundation.Samplers.Length;
                Array.Resize(ref foundation.Samplers, slotID + 1);
                for (int i = oldLength; i < foundation.Samplers.Length; i++)
                    foundation.Samplers[i] = foundation.Samplers[0];
            }

            foundation.Samplers[slotID] = foundation.Device.CreateSampler(ref parameters);
        }
    }
}
