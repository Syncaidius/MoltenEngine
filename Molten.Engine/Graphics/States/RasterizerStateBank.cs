namespace Molten.Graphics
{
    public class RasterizerStateBank : GraphicsStateBank<GraphicsRasterizerState, RasterizerPreset>
    {
        internal RasterizerStateBank(GraphicsDevice device)
        {
            GraphicsRasterizerState state = device.CreateRasterizerState();
            AddPreset(RasterizerPreset.Default, state);

            // Wireframe preset.
            state = device.CreateRasterizerState();
            state.FillingMode = RasterizerFillingMode.Wireframe;
            AddPreset(RasterizerPreset.Wireframe, state);

            // Scissor test preset
            state = device.CreateRasterizerState();
            state.IsScissorEnabled = true;
            AddPreset(RasterizerPreset.ScissorTest, state);

            // No culling preset.
            state = device.CreateRasterizerState();
            state.CullingMode = RasterizerCullingMode.None;
            AddPreset(RasterizerPreset.NoCulling, state);

            // Multi-sampling preset - Default
            state = device.CreateRasterizerState();
            state.IsMultisampleEnabled = true;
            AddPreset(RasterizerPreset.DefaultMultisample, state);

            state = device.CreateRasterizerState();
            state.IsScissorEnabled = true;
            state.IsMultisampleEnabled = true;
            AddPreset(RasterizerPreset.ScissorTestMultisample, state);
        }

        public override GraphicsRasterizerState GetPreset(RasterizerPreset value)
        {
            return _presets[(int)value];
        }
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
}
