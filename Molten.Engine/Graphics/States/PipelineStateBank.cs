namespace Molten.Graphics
{
    public class PipelineStateBank : GraphicsStateBank<GraphicsPipelineState, PipelineStatePreset>
    {
        GraphicsDevice _device;

        internal PipelineStateBank(GraphicsDevice device)
        {
            _device = device;
        }

        public override void ApplyPreset(GraphicsPipelineState state, PipelineStatePreset preset)
        {
            switch (preset)
            {
                default:
                case PipelineStatePreset.Default:
                    ApplyBlendPreset(state, BlendPreset.Default);
                    ApplyRasterizerPreset(state, RasterizerPreset.Default);
                    ApplyDepthPreset(state, DepthStencilPreset.Default);
                    break;
            }
        }

        public void ApplyBlendPreset(GraphicsPipelineState state, BlendPreset preset, int blendSlot = -1)
        {
            state.BlendFactor = new Color4(1, 1, 1, 1);
            state.BlendSampleMask = 0xffffffff;
            state.AlphaToCoverageEnable = false;
            state.IndependentBlendEnable = false;

            int slot = blendSlot >= 0 ? blendSlot : 0;
            GraphicsPipelineState.RenderSurfaceBlend b = state[slot];

            b.SrcBlend = BlendType.One;
            b.DestBlend = BlendType.Zero;
            b.BlendOp = BlendOperation.Add;
            b.SrcBlendAlpha = BlendType.One;
            b.DestBlendAlpha = BlendType.Zero;
            b.BlendOpAlpha = BlendOperation.Add;
            b.RenderTargetWriteMask = ColorWriteFlags.All;
            b.BlendEnable = true;
            b.LogicOp = LogicOperation.Noop;
            b.LogicOpEnable = false;

            switch (preset)
            {
                case BlendPreset.Additive:
                    b.SrcBlend = BlendType.One;
                    b.DestBlend = BlendType.One;
                    b.BlendOp = BlendOperation.Add;
                    b.SrcBlendAlpha = BlendType.One;
                    b.DestBlendAlpha = BlendType.One;
                    b.BlendOpAlpha = BlendOperation.Add;
                    b.RenderTargetWriteMask = ColorWriteFlags.All;
                    b.BlendEnable = true;
                    b.LogicOp = LogicOperation.Noop;
                    b.LogicOpEnable = false;
                    state.AlphaToCoverageEnable = false;
                    state.IndependentBlendEnable = false;
                    break;

                case BlendPreset.PreMultipliedAlpha:
                    b.SrcBlend = BlendType.SrcAlpha;
                    b.DestBlend = BlendType.InvSrcAlpha;
                    b.BlendOp = BlendOperation.Add;
                    b.SrcBlendAlpha = BlendType.InvDestAlpha;
                    b.DestBlendAlpha = BlendType.One;
                    b.BlendOpAlpha = BlendOperation.Add;
                    b.RenderTargetWriteMask = ColorWriteFlags.All;
                    b.BlendEnable = true;
                    b.LogicOp = LogicOperation.Noop;
                    b.LogicOpEnable = false;
                    state.AlphaToCoverageEnable = false;
                    state.IndependentBlendEnable = false;
                    break;
            }

            // Apply the blend to the other slots, unless specified.
            if (blendSlot >= 0)
            {
                for (int i = 0; i < state.SurfaceBlendCount; i++)
                {
                    if (i != slot)
                        state[i].Set(b);
                }
            }
        }

        public void ApplyDepthPreset(GraphicsPipelineState state, DepthStencilPreset preset)
        {
            // Based on the default DX11 values: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_depth_stencil_desc
            // Revert to defaults first
            state.IsDepthEnabled = true;
            state.DepthWriteEnabled = true;
            state.DepthComparison = ComparisonFunction.Less;
            state.IsStencilEnabled = true;
            state.StencilReadMask = 255;
            state.StencilWriteMask = 255;
            state.FrontFace.Comparison = ComparisonFunction.Always;
            state.FrontFace.DepthFail = DepthStencilOperation.Keep;
            state.FrontFace.StencilPass = DepthStencilOperation.Keep;
            state.FrontFace.StencilFail = DepthStencilOperation.Keep;
            state.BackFace.Set(state.FrontFace);

            // Now apply customizations
            switch (preset)
            {
                case DepthStencilPreset.DefaultNoStencil:
                    state.IsStencilEnabled = false;
                    break;

                case DepthStencilPreset.ZDisabled:
                    state.IsStencilEnabled = false;
                    state.IsDepthEnabled = false;
                    state.DepthWriteEnabled = false;
                    break;
            }
        }

        public void ApplyRasterizerPreset(GraphicsPipelineState state, RasterizerPreset preset)
        {
            // Revert to defaults first
            state.Fill = RasterizerFillingMode.Solid;
            state.Cull = RasterizerCullingMode.Back;
            state.IsFrontCounterClockwise = false;
            state.DepthBias = 0;
            state.SlopeScaledDepthBias = 0.0f;
            state.DepthBiasClamp = 0.0f;
            state.IsDepthClipEnabled = true;
            state.IsScissorEnabled = false;
            state.IsMultisampleEnabled = false;
            state.IsAALineEnabled = false;
            state.ConservativeRaster = ConservativeRasterizerMode.Off;
            state.ForcedSampleCount = 0;

            // Now apply customizations
            switch (preset)
            {
                case RasterizerPreset.Wireframe:
                    state.Fill = RasterizerFillingMode.Wireframe;
                    break;

                case RasterizerPreset.ScissorTest:
                    state.IsScissorEnabled = true;
                    break;

                case RasterizerPreset.NoCulling:
                    state.Cull = RasterizerCullingMode.None;
                    break;

                case RasterizerPreset.DefaultMultisample:
                    state.IsMultisampleEnabled = true;
                    break;

                case RasterizerPreset.ScissorTestMultisample:
                    state.IsScissorEnabled = true;
                    state.IsMultisampleEnabled = true;
                    break;
            }
        }
    }

    public enum PipelineStatePreset
    {
        Default = 0,
    }
}
