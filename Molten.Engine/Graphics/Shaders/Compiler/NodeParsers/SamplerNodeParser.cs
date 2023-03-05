namespace Molten.Graphics
{
    internal class SamplerNodeParser : ShaderNodeParser<HlslPass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Sampler;

        protected override void OnParse(HlslPass pass, ShaderCompilerContext context, ShaderHeaderNode node)
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
            if (slotID >= pass.Samplers.Length)
            {
                int oldLength = pass.Samplers.Length;
                Array.Resize(ref pass.Samplers, slotID + 1);
                for (int i = oldLength; i < pass.Samplers.Length; i++)
                    pass.Samplers[i] = pass.Samplers[0];
            }

            pass.Samplers[slotID] = pass.Device.CreateSampler(ref parameters);
        }
    }
}
