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

    public enum BlendPreset
    {
        /// <summary>The default blend mode.</summary>
        Default = 0,

        /// <summary>Additive blending mode.</summary>
        Additive = 1,

        /// <summary>Pre-multiplied alpha blending mode.</summary>
        PreMultipliedAlpha = 2,
    }

    /// <summary>Represents several rasterizer state presets.</summary>
    public enum RasterizerPreset
    {
        /// <summary>The default rasterizer state.</summary>
        Default = 0,

        /// <summary>The same as the default rasterizer state, but with wireframe enabled.</summary>
        Wireframe = 1,

        /// <summary>The same as the default rasterizer state, but with scissor testing enabled.</summary>
        ScissorTest = 2,

        /// <summary>Culling is disabled. Back and front faces will be drawn.</summary>
        NoCulling = 3,

        /// <summary>
        /// The same as <see cref="Default"/> but with multisampling enabled.
        /// </summary>
        DefaultMultisample = 4,

        /// <summary>
        /// The same as <see cref="ScissorTest"/> but with multisampling enabled.
        /// </summary>
        ScissorTestMultisample = 5,
    }

    public enum DepthStencilPreset
    {
        /// <summary>Default depth stencil state with stencil testing enabled.</summary>
        Default = 0,

        /// <summary>The default depth stencil state, but with stencil testing disabled.</summary>
        DefaultNoStencil = 1,

        /// <summary>The same as default, but with the z-buffer disabled.</summary>
        ZDisabled = 2,
    }
}
