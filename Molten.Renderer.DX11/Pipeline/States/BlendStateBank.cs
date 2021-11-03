using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class BlendStateBank : GraphicsStateBank<GraphicsBlendState, BlendPreset>
    {
        internal BlendStateBank(DeviceDX11 device)
        {
            AddPreset(BlendPreset.Default, new GraphicsBlendState(device));

            // Additive blending preset.
            GraphicsBlendState state = new GraphicsBlendState(device, new RenderTargetBlendDescription()
            {
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                IsBlendEnabled = true,
            })
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,

            };
            AddPreset(BlendPreset.Additive, state);

            // Pre-multiplied alpha
            state = new GraphicsBlendState(device, new RenderTargetBlendDescription()
            {
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,

                SourceAlphaBlend = BlendOption.InverseDestinationAlpha,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,

                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                IsBlendEnabled = true,
            })
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            AddPreset(BlendPreset.PreMultipliedAlpha, state);
        }

        internal override GraphicsBlendState GetPreset(BlendPreset value)
        {
            return _presets[(int)value];
        }
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
}
