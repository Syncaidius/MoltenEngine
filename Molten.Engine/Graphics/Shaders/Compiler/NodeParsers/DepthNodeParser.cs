using System.Reflection;

namespace Molten.Graphics
{
    internal class DepthNodeParser : StateNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Depth;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            DepthStencilPreset preset = DepthStencilPreset.Default;

            if (node.ValueType == ShaderHeaderValueType.Preset)
            {
                if (!Enum.TryParse(node.Value, true, out preset))
                    InvalidEnumMessage<DepthStencilPreset>(context, (node.Name, node.Value), "depth-stencil preset");
            }

            GraphicsDepthState template = foundation.DepthState[node.Conditions] ?? foundation.Device.DepthBank.GetPreset(preset);
            GraphicsDepthState state = foundation.Device.CreateDepthState(template);
            ParseProperties(node, context, state);

            state = foundation.Device.DepthBank.AddOrRetrieveExisting(state);

            if (node.Conditions == StateConditions.None)
                foundation.DepthState.FillMissingWith(state);
            else
                foundation.DepthState[node.Conditions] = state;
        }
    }
}
