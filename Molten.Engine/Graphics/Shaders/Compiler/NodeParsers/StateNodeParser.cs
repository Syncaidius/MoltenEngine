namespace Molten.Graphics
{
    internal class StateNodeParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.State;

        protected override void OnParse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (passDef == null)
                return;

            // Use the default preset's Surface0 blend description.
            GraphicsStatePreset preset = GraphicsStatePreset.Default;
            ShaderPassParameters passParams = new ShaderPassParameters(preset, PrimitiveTopology.Triangle);

            if (node.Values.TryGetValue(ShaderHeaderValueType.Preset, out string presetValue))
            {
                if (Enum.TryParse(presetValue, true, out preset))
                {
                    if (preset != GraphicsStatePreset.Default)
                        passParams.ApplyPreset(preset);
                }
                else
                {
                    InvalidEnumMessage<GraphicsStatePreset>(context, (node.Name, presetValue), "pipeline preset");
                }
            }

            // Check for blend preset
            if (node.Values.TryGetValue(ShaderHeaderValueType.BlendPreset, out string blendValue))
            {
                if (Enum.TryParse(blendValue, true, out BlendPreset subPreset))
                    passParams.ApplyBlendPreset(subPreset);
                else
                    InvalidEnumMessage<BlendPreset>(context, (node.Name, blendValue), "blend preset");
            }

            // Check for rasterizer preset
            if (node.Values.TryGetValue(ShaderHeaderValueType.RasterizerPreset, out string rasterValue))
            {
                if (Enum.TryParse(rasterValue, true, out RasterizerPreset subPreset))
                    passParams.ApplyRasterizerPreset(subPreset);
                else
                    InvalidEnumMessage<RasterizerPreset>(context, (node.Name, rasterValue), "rasterizer preset");
            }

            // Check for depth-stencil preset
            if (node.Values.TryGetValue(ShaderHeaderValueType.DepthPreset, out string depthValue))
            {
                if (Enum.TryParse(depthValue, true, out DepthStencilPreset subPreset))
                    passParams.ApplyDepthPreset(subPreset);
                else
                    InvalidEnumMessage<DepthStencilPreset>(context, (node.Name, depthValue), "depth-stencil preset");
            }

            ParseFields(node, context, ref passParams);
            passParams.IndependentBlendEnable = false;
            for (int i = 0; i < ShaderPassParameters.MAX_SURFACES; i++)
            {
                if (passParams[i].BlendEnable && i > 0)
                {
                    passParams.IndependentBlendEnable = true;
                    break;
                }
            }

            passDef.Parameters = passParams;
        }
    }
}
