using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DepthPresetBank : GraphicsPresetBank<GraphicsDepthState, DepthStencilPreset>
    {
        internal DepthPresetBank()
        {
            AddPreset(DepthStencilPreset.Default, new GraphicsDepthState()
            {
                IsStencilEnabled = true,
            });

            // Default preset
            AddPreset(DepthStencilPreset.DefaultNoStencil, new GraphicsDepthState());

            // Z-disabled preset
            AddPreset(DepthStencilPreset.ZDisabled, new GraphicsDepthState()
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
            });

            AddPreset(DepthStencilPreset.Sprite2D, new GraphicsDepthState()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = true,
                DepthComparison = Comparison.LessEqual,
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
