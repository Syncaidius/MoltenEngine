namespace Molten.Graphics
{
    public class DepthStateBank : GraphicsStateBank<GraphicsDepthState, DepthStencilPreset>
    {
        GraphicsDevice _device;

        internal DepthStateBank(GraphicsDevice device)
        {
            _device = device;

            GraphicsDepthState state = device.CreateDepthState();
            state.IsStencilEnabled = true;
            AddPreset(DepthStencilPreset.Default, state);

            // Default preset
            state = device.CreateDepthState();
            AddPreset(DepthStencilPreset.DefaultNoStencil, state);

            // Z-disabled preset
            state = device.CreateDepthState();
            state.IsDepthEnabled = false;
            state.WriteFlags = DepthWriteFlags.Zero;
            AddPreset(DepthStencilPreset.ZDisabled, state);
        }

        public override GraphicsDepthState GetPreset(DepthStencilPreset value)
        {
            return _presets[(int)value];
        }
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
