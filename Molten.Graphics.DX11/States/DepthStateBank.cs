using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class DepthStateBank : GraphicsStateBank<GraphicsDepthState, DepthStencilPreset>
    {
        internal DepthStateBank(Device device)
        {
            AddPreset(DepthStencilPreset.Default, new GraphicsDepthState(device)
            {
                IsStencilEnabled = true,
            });

            // Default preset
            AddPreset(DepthStencilPreset.DefaultNoStencil, new GraphicsDepthState(device));

            // Z-disabled preset
            AddPreset(DepthStencilPreset.ZDisabled, new GraphicsDepthState(device)
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.DepthWriteMaskZero,
            });

            AddPreset(DepthStencilPreset.Sprite2D, new GraphicsDepthState(device)
            {
                IsDepthEnabled = true,
                IsStencilEnabled = true,
                DepthComparison = ComparisonFunc.ComparisonLessEqual,
            });
        }

        internal override GraphicsDepthState GetPreset(DepthStencilPreset value)
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

        /// <summary>A state used for drawing 2D sprites. Stenicl testing is enabled.</summary>
        Sprite2D = 3,
    }
}
