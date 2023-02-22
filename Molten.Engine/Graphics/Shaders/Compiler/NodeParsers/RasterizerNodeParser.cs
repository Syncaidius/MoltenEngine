namespace Molten.Graphics
{
    internal class RasterizerNodeParser : StateNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Rasterizer;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            RasterizerPreset preset = RasterizerPreset.Default;

            if (node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (!Enum.TryParse(node.Value, true, out preset))
                    InvalidEnumMessage<RasterizerPreset>(context, (node.Name, node.Value), "rasterizer preset");
            }

            GraphicsRasterizerState template = foundation.RasterizerState[node.Conditions] ?? foundation.Device.RasterizerBank.GetPreset(preset);
            GraphicsRasterizerState state = foundation.Device.CreateRasterizerState(template);
            ParseProperties(node, context, state);

            state = foundation.Device.RasterizerBank.AddOrRetrieveExisting(state);

            if (node.Conditions == StateConditions.None)
                foundation.RasterizerState.FillMissingWith(state);
            else
                foundation.RasterizerState[node.Conditions] = state;
        }
    }
}
