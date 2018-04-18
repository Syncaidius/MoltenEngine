using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class BlendPresetBank : GraphicsPresetBank<GraphicsBlendState, BlendingPreset>
    {
        internal BlendPresetBank()
        {
            AddPreset(BlendingPreset.Default, new GraphicsBlendState());

            // Additive blending preset.
            AddPreset(BlendingPreset.Additive, new GraphicsBlendState()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
            });

            // Pre-multiplied alpha
            AddPreset(BlendingPreset.PreMultipliedAlpha, new GraphicsBlendState()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,

                SourceAlphaBlend = BlendOption.InverseDestinationAlpha,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,

                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                IsBlendEnabled = true,
            });
        }

        internal override GraphicsBlendState GetPreset(BlendingPreset value)
        {
            return _presets[(int)value];
        }
    }

    public enum BlendingPreset
    {
        /// <summary>The default blend mode.</summary>
        Default = 0,

        /// <summary>Additive blending mode.</summary>
        Additive = 1,

        /// <summary>Pre-multiplied alpha blending mode.</summary>
        PreMultipliedAlpha = 2,
    }
}
