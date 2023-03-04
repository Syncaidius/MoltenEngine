namespace Molten.Graphics
{
    internal class StateNodeParser : ShaderNodeParser
    {
        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        public override ShaderNodeType NodeType => ShaderNodeType.State;

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            // Use the default preset's Surface0 blend description.
            GraphicsStatePreset preset = GraphicsStatePreset.Default;
            GraphicsStateParameters gsp = new GraphicsStateParameters(preset, PrimitiveTopology.Triangle);

            if(node.Values.TryGetValue(ShaderHeaderValueType.Preset, out string presetValue))
            {
                if (Enum.TryParse(presetValue, true, out preset))
                {
                    if (preset != GraphicsStatePreset.Default)
                        gsp.ApplyPreset(preset);
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
                        gsp.ApplyBlendPreset(subPreset);
                else
                    InvalidEnumMessage<BlendPreset>(context, (node.Name, blendValue), "blend preset");
            }

            // Check for rasterizer preset
            if (node.Values.TryGetValue(ShaderHeaderValueType.RasterizerPreset, out string rasterValue))
            {
                if (Enum.TryParse(rasterValue, true, out RasterizerPreset subPreset))
                        gsp.ApplyRasterizerPreset(subPreset);
                else
                    InvalidEnumMessage<RasterizerPreset>(context, (node.Name, rasterValue), "rasterizer preset");
            }

            // Check for depth-stencil preset
            if (node.Values.TryGetValue(ShaderHeaderValueType.DepthPreset, out string depthValue))
            {
                if (Enum.TryParse(depthValue, true, out DepthStencilPreset subPreset))
                        gsp.ApplyDepthPreset(subPreset);
                else
                    InvalidEnumMessage<DepthStencilPreset>(context, (node.Name, depthValue), "depth-stencil preset");
            }

            ParseFields(node, context, ref gsp);
            gsp.IndependentBlendEnable = false;
            for(int i = 0; i < GraphicsStateParameters.MAX_SURFACES; i++)
            {
                if (gsp[i].BlendEnable && i > 0)
                {
                    gsp.IndependentBlendEnable = true;
                    break;
                }
            }

            switch (foundation)
            {
                case Material mat:
                    mat.DefaultState = foundation.Device.CreateState(ref gsp);
                    break;

                case MaterialPass pass:
                    pass.State = foundation.Device.CreateState(ref gsp);
                    break;
            }
        }
    }
}
